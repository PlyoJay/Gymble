using Dapper;
using Gymble.Models;
using Gymble.Services;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gymble.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        private readonly Func<SQLiteConnection> _connFactory;

        public MemberRepository(Func<SQLiteConnection> connFactory)
            => _connFactory = connFactory;

        public async Task<long> InsertMemberAsync(Member member, CancellationToken ct = default)
        {
            using var conn = _connFactory();

            const string sql = @"
                INSERT INTO tb_member (name, gender, phone_number, birthdate, register_date, state, memo)
                VALUES (@Name, @Gender, @PhoneNumber, @BirthDate, @RegisterDate, @State, @Memo);
                SELECT last_insert_rowid();";

            var cmd = new CommandDefinition(sql, member, cancellationToken: ct);
            return await conn.ExecuteScalarAsync<long>(cmd);
        }

        public async Task<int> UpdateMemberAsync(Member member, CancellationToken ct = default)
        {
            using var conn = _connFactory();

            const string sql = @"
                UPDATE tb_member
                SET name = @Name,
                    gender = @Gender,
                    phone_number = @PhoneNumber,
                    birthdate = @BirthDate,
                    register_date = @RegisterDate,
                    state = @State,
                    memo = @Memo
                WHERE id = @Id;";

            var cmd = new CommandDefinition(sql, member, cancellationToken: ct);
            return await conn.ExecuteAsync(cmd);
        }

        public async Task<int> DeleteMemberAsync(Member member, CancellationToken ct = default)
        {
            using var conn = _connFactory();

            // 추천: 소프트 삭제 컬럼이 있으면 UPDATE is_deleted=1 로 바꾸는 게 더 안전함.
            const string sql = "DELETE FROM tb_member WHERE id = @Id;";
            var cmd = new CommandDefinition(sql, new { Id = member.Id }, cancellationToken: ct);
            return await conn.ExecuteAsync(cmd);
        }

        public async Task<Member> GetByIdAsync(long id, CancellationToken ct = default)
        {
            using var conn = _connFactory();

            const string sql = "SELECT * FROM tb_member WHERE id = @Id;";
            var cmd = new CommandDefinition(sql, new { Id = id }, cancellationToken: ct);
            return await conn.QuerySingleAsync<Member>(cmd);
        }

        public async Task<IReadOnlyList<Member>> GetAllAsync(CancellationToken ct = default)
        {
            using var conn = _connFactory();

            const string sql = @"
                SELECT
                  id,
                  name,
                  gender,
                  phone_number AS PhoneNumber,
                  birthdate,
                  register_date AS RegisterDate,
                  state AS State,
                  memo
                FROM tb_member;
                ";
            var cmd = new CommandDefinition(sql, cancellationToken: ct);
            var rows = await conn.QueryAsync<Member>(cmd);
            return rows.AsList();
        }

        public async Task<PagedResult<Member>> SearchAsync(MemberSearch q, CancellationToken ct = default)
        {
            using var conn = _connFactory();

            q ??= new MemberSearch();

            // SortBy 화이트리스트 (SQL Injection 방지)
            string orderBy = q.SortBy switch
            {
                "name" => "name",
                "phone_number" => "phone_number",
                _ => "register_date"
            };
            string dir = q.Desc ? "DESC" : "ASC";

            var where = new List<string>();
            var p = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(q.NameOrPhone))
            {
                where.Add("(name LIKE @kw OR phone_number LIKE @kw)");
                p.Add("@kw", $"%{q.NameOrPhone}%");
            }
            if (q.RegFrom is not null)
            {
                where.Add("register_date >= @from");
                p.Add("@from", q.RegFrom.Value);
            }
            if (q.RegTo is not null)
            {
                where.Add("register_date < @to");
                p.Add("@to", q.RegTo.Value);
            }

            string whereSql = where.Count == 0 ? "" : "WHERE " + string.Join(" AND ", where);

            int page = Math.Max(1, q.Page);
            int pageSize = Math.Clamp(q.PageSize, 10, 500);
            int offset = (page - 1) * pageSize;

            // 1) total
            string sqlTotal = $"SELECT COUNT(1) FROM tb_member {whereSql};";
            // 2) rows
            string sqlRows = $@"
                SELECT *
                FROM tb_member
                {whereSql}
                ORDER BY {orderBy} {dir}
                LIMIT @take OFFSET @skip;";

            p.Add("@take", pageSize);
            p.Add("@skip", offset);

            var totalCmd = new CommandDefinition(sqlTotal, p, cancellationToken: ct);
            var rowsCmd = new CommandDefinition(sqlRows, p, cancellationToken: ct);

            int total = await conn.ExecuteScalarAsync<int>(totalCmd);
            var rows = (await conn.QueryAsync<Member>(rowsCmd)).AsList();

            return new PagedResult<Member>
            {
                Rows = rows,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
