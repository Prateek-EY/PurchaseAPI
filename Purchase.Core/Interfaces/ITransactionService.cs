using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Purchase.Core.Entities;

namespace Purchase.Core.Interfaces
{
    public interface ITransactionService
    {
        Task<PurchaseTransaction> CreateTransactionAsync(PurchaseTransaction transaction);
        Task<PurchaseTransaction?> GetTransactionAsync(Guid id);
        Task<List<PurchaseTransaction>> GetAllTransactionsAsync();
    }
}
