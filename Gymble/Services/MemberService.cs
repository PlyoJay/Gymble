using Gymble.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gymble.Services
{
    public interface IMemberService
    {
        Task<IReadOnlyList<Member>> GetAllAsync(CancellationToken ct = default);
        Task<PagedResult<Member>> SearchAsync(MemberSearch q, CancellationToken ct = default);

        Task<long> AddAsync(Member member, CancellationToken ct = default);
        Task UpdateAsync(Member member, CancellationToken ct = default);
        Task DeleteAsync(Member member, CancellationToken ct = default);
    }

    public sealed class MemberService : IMemberService
    {
        private readonly IMemberRepository _repo;

        public MemberService(IMemberRepository repo)
            => _repo = repo ?? throw new ArgumentNullException(nameof(repo));

        public Task<IReadOnlyList<Member>> GetAllAsync(CancellationToken ct = default)
            => _repo.GetAllAsync(ct);

        public Task<PagedResult<Member>> SearchAsync(MemberSearch q, CancellationToken ct = default)
        {
            q ??= new MemberSearch();
            return _repo.SearchAsync(q, ct);
        }

        public async Task<long> AddAsync(Member member, CancellationToken ct = default)
        {
            Validate(member, isNew: true);

            // 기본값 보정(원하면 더 추가)
            if (member.RegisterDate == default)
                member.RegisterDate = DateTime.Now; // DB에는 로컬시간 저장 중이라면 일단 Now로

            // 전화번호 정규화(옵션)
            member.PhoneNumber = NormalizePhone(member.PhoneNumber);

            return await _repo.InsertMemberAsync(member, ct);
        }

        public async Task UpdateAsync(Member member, CancellationToken ct = default)
        {
            Validate(member, isNew: false);
            member.PhoneNumber = NormalizePhone(member.PhoneNumber);

            var affected = await _repo.UpdateMemberAsync(member, ct);
            if (affected == 0)
                throw new InvalidOperationException("수정 대상 회원이 없습니다.");
        }

        public async Task DeleteAsync(Member member, CancellationToken ct = default)
        {
            if (member == null || member.Id <= 0)
                throw new ArgumentException("삭제할 회원이 올바르지 않습니다.");

            var affected = await _repo.DeleteMemberAsync(member, ct);
            if (affected == 0)
                throw new InvalidOperationException("삭제 대상 회원이 없습니다.");
        }

        private static void Validate(Member m, bool isNew)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));

            if (!isNew && m.Id <= 0)
                throw new ArgumentException("회원 ID가 올바르지 않습니다.");

            if (string.IsNullOrWhiteSpace(m.Name))
                throw new ArgumentException("이름은 필수입니다.");

            if (string.IsNullOrWhiteSpace(m.PhoneNumber))
                throw new ArgumentException("전화번호는 필수입니다.");

            // BirthDate는 optional이면 여기서 강제하지 말고,
            // 필수로 쓸 거면 default 체크 넣어도 됨.
        }

        private static string? NormalizePhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return phone;
            // 숫자만 남기기(010-1234-5678 -> 01012345678)
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            return digits;
        }
    }
}
