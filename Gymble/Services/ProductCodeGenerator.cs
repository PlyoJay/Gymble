using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gymble.Services
{
    public interface IProductCodeGenerator
    {
        Task<string> GenerateAsync(ProductCategory category, CancellationToken ct = default);
    }


    public sealed class ProductCodeGenerator : IProductCodeGenerator
    {
        private readonly Func<SQLiteConnection> _connFactory;

        public ProductCodeGenerator(Func<SQLiteConnection> connFactory)
        {
            _connFactory = connFactory;
        }

        public async Task<string> GenerateAsync(ProductCategory category, CancellationToken ct = default)
        {
            string prefix = category switch
            {
                ProductCategory.Gym => "GYM",
                ProductCategory.PT => "PT",
                ProductCategory.Locker => "LOCK",
                ProductCategory.Wear => "WEAR",
                _ => "ETC"
            };

            using var conn = _connFactory();

            using var tx = conn.BeginTransaction();

            // 1) prefix row가 없으면 생성(0으로 시작)
            const string insertIfMissing = @"
                INSERT OR IGNORE INTO tb_code_sequence(prefix, last_value)
                VALUES (@Prefix, 0);
            ";

            await conn.ExecuteAsync(new CommandDefinition(
                insertIfMissing, new { Prefix = prefix }, transaction: tx, cancellationToken: ct));

            // 2) last_value 증가
            const string update = @"
                UPDATE tb_code_sequence
                SET last_value = last_value + 1
                WHERE prefix = @Prefix;
            ";

            await conn.ExecuteAsync(new CommandDefinition(
                update, new { Prefix = prefix }, transaction: tx, cancellationToken: ct));

            // 3) 증가된 값 읽기
            const string select = @"
                SELECT last_value
                FROM tb_code_sequence
                WHERE prefix = @Prefix;
            ";

            int seq = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
                select, new { Prefix = prefix }, transaction: tx, cancellationToken: ct));

            tx.Commit();

            // 4) 최종 코드 포맷
            return $"{prefix}-{seq:0000}";
        }
    }
}
