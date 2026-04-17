using Gymble.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gymble.Services
{
    public interface IPurchaseService
    {
        Task<int> CreatePurchaseAsync(PurchaseRequest request);
    }

    public class PurchaseService : IPurchaseService
    {
        public Task<int> CreatePurchaseAsync(PurchaseRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
