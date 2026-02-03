using Gymble.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gymble.Services
{
    public interface IAttendanceService
    {
        Task<IReadOnlyList<Member>> GetAllAsync(CancellationToken ct = default);
        Task<PagedResult<Member>> SearchAsync(MemberSearch q, CancellationToken ct = default);

        Task<long> AddAsync(Member member, CancellationToken ct = default);
        Task UpdateAsync(Member member, CancellationToken ct = default);
        Task DeleteAsync(Member member, CancellationToken ct = default);
    }

    class AttendanceService
    {
    }
}
