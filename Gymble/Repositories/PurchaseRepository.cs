using Dapper;
using Gymble.Models;
using Gymble.Services;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gymble.Repositories
{
    public interface IPurchaseRepository
    {
        Task<int> InsertPurchaseAsync(Purchase purchase);
        Task<int> InsertPurchaseItemAsync(PurchaseItem item);
        Task<int> InsertMemberMembershipAsync(MemberMembership membership);

        Task<int> InsertPurchaseAsync(SQLiteConnection conn, SQLiteTransaction tx, Purchase purchase);
        Task<int> InsertPurchaseItemAsync(SQLiteConnection conn, SQLiteTransaction tx, PurchaseItem item);
        Task<int> InsertMemberMembershipAsync(SQLiteConnection conn, SQLiteTransaction tx, MemberMembership membership);
    }

    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly Func<SQLiteConnection> _connFactory;

        public PurchaseRepository(Func<SQLiteConnection> connFactory)
            => _connFactory = connFactory;


        public async Task<int> InsertPurchaseAsync(Purchase purchase)
        {
            using var conn = _connFactory();
            return await conn.ExecuteScalarAsync<int>(SqlPurchaseQuery.INSERT_PURCHASE, purchase);
        }

        public async Task<int> InsertPurchaseItemAsync(PurchaseItem item)
        {
            using var conn = _connFactory();
            return await conn.ExecuteScalarAsync<int>(SqlPurchaseQuery.INSERT_PURCHASE_ITEM, item);
        }

        public async Task<int> InsertMemberMembershipAsync(MemberMembership membership)
        {
            using var conn = _connFactory();
            return await conn.ExecuteScalarAsync<int>(SqlMemberMembershipQuery.INSERT_MEMBER_MEMBERSHIP, membership);
        }

        public async Task<int> InsertPurchaseAsync(SQLiteConnection conn, SQLiteTransaction tx, Purchase purchase)
        {
            return await conn.ExecuteScalarAsync<int>(
                SqlPurchaseQuery.INSERT_PURCHASE,
                purchase,
                tx);
        }

        public async Task<int> InsertPurchaseItemAsync(SQLiteConnection conn, SQLiteTransaction tx, PurchaseItem item)
        {
            return await conn.ExecuteScalarAsync<int>(
                SqlPurchaseQuery.INSERT_PURCHASE_ITEM,
                item,
                tx);
        }

        public async Task<int> InsertMemberMembershipAsync(SQLiteConnection conn, SQLiteTransaction tx, MemberMembership membership)
        {
            return await conn.ExecuteScalarAsync<int>(
                SqlMemberMembershipQuery.INSERT_MEMBER_MEMBERSHIP,
                membership,
                tx);
        }
    }
}
