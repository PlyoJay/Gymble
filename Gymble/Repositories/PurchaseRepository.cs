using Gymble.Models;
using System;
using System.Collections.Generic;
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

        Task<Purchase?> GetPurchaseByIdAsync(int id);
        Task<List<PurchaseItem>> GetPurchaseItemsAsync(int purchaseId);
        Task<List<MemberMembership>> GetMembershipsByPurchaseIdAsync(int purchaseId);
    }

    public class PurchaseRepository : IPurchaseRepository
    {
        public Task<List<MemberMembership>> GetMembershipsByPurchaseIdAsync(int purchaseId)
        {
            throw new NotImplementedException();
        }

        public Task<Purchase?> GetPurchaseByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<PurchaseItem>> GetPurchaseItemsAsync(int purchaseId)
        {
            throw new NotImplementedException();
        }

        public Task<int> InsertMemberMembershipAsync(MemberMembership membership)
        {
            throw new NotImplementedException();
        }

        public Task<int> InsertPurchaseAsync(Purchase purchase)
        {
            throw new NotImplementedException();
        }

        public Task<int> InsertPurchaseItemAsync(PurchaseItem item)
        {
            throw new NotImplementedException();
        }
    }
}
