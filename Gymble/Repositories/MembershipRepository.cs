using Dapper;
using Gymble.Models;
using Gymble.Services;
using System.Data.SQLite;

namespace Gymble.Repositories
{
    public interface IMembershipRepository
    {
        Task<IReadOnlyList<MemberMembership>> GetByMemberIdAsync(long memberId, CancellationToken ct = default);
        Task<int> UpdateActivationAsync(MemberMembership membership, CancellationToken ct = default);
    }

    public sealed class MembershipRepository : IMembershipRepository
    {
        private readonly Func<SQLiteConnection> _connFactory;

        public MembershipRepository(Func<SQLiteConnection> connFactory)
            => _connFactory = connFactory ?? throw new ArgumentNullException(nameof(connFactory));

        public async Task<IReadOnlyList<MemberMembership>> GetByMemberIdAsync(long memberId, CancellationToken ct = default)
        {
            using var conn = _connFactory();

            var cmd = new CommandDefinition(
                SqlMemberMembershipQuery.GET_MEMBER_MEMBERSHIPS_BY_MEMBER_ID,
                new { MemberId = memberId },
                cancellationToken: ct);

            return (await conn.QueryAsync<MemberMembership>(cmd)).AsList();
        }

        public async Task<int> UpdateActivationAsync(MemberMembership membership, CancellationToken ct = default)
        {
            using var conn = _connFactory();

            var cmd = new CommandDefinition(
                SqlMemberMembershipQuery.UPDATE_MEMBER_MEMBERSHIP_ACTIVATION,
                membership,
                cancellationToken: ct);

            return await conn.ExecuteAsync(cmd);
        }
    }
}
