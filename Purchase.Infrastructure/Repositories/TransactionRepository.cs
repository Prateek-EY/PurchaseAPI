using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Purchase.Core.Entities;
using Purchase.Core.Interfaces;
using Purchase.Core.Request;
using Purchase.Infrastructure.Data;

namespace Purchase.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TransactionRepository> logger;

        public TransactionRepository(AppDbContext context, ILogger<TransactionRepository> _logger)
        {
            _context = context;
            logger = _logger;
        }

        public async Task<PurchaseTransaction> AddAsync(PurchaseTransactionRequest transaction)
        {
            try
            {
                if (transaction.AmountUSD < 0)
                {
                    throw new Exception("Amount is negative");
                }

                var purchaseTransaction = new PurchaseTransaction
                {
                    Id = transaction.Id,
                    Description = transaction.Description,
                    TransactionDate = transaction.TransactionDate,
                    AmountUSD = transaction.AmountUSD
                };

                _context.Transactions.Add(purchaseTransaction);
                await _context.SaveChangesAsync();
                return purchaseTransaction;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<PurchaseTransaction?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Transactions.FindAsync(id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<List<PurchaseTransaction>> GetAllAsync()
        {
            try
            {
                return await _context.Transactions.ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<List<PurchaseTransaction>> GetByIdsAsync(List<Guid> transactionIds)
        {
            try
            {
                return await _context.Transactions
                    .Where(t => transactionIds.Contains(t.Id))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
