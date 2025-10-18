using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Purchase.Core.Entities;

namespace Purchase.Core.Interfaces
{
    public interface ITransactionRepository
    {
        Task<PurchaseTransaction> AddAsync(PurchaseTransaction transaction);
        Task<PurchaseTransaction?> GetByIdAsync(Guid id);
        Task<List<PurchaseTransaction>> GetAllAsync();
    }
}
