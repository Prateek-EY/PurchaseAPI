using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Purchase.Core.Entities;
using Purchase.Core.Request;

namespace Purchase.Core.Interfaces
{
    public interface ITransactionRepository
    {
        Task<PurchaseTransaction> AddAsync(PurchaseTransactionRequest transaction);
        Task<PurchaseTransaction?> GetByIdAsync(Guid id);
        Task<List<PurchaseTransaction>> GetAllAsync();
        Task<List<PurchaseTransaction>> GetByIdsAsync(List<Guid> transactionIds);
    }
}
