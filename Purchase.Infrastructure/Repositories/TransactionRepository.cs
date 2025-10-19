using Microsoft.EntityFrameworkCore;
using Purchase.Core.Entities;
using Purchase.Core.Interfaces;
using Purchase.Infrastructure.Data;

namespace Purchase.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;

        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PurchaseTransaction> AddAsync(PurchaseTransaction transaction)
        {
            try
            {
                transaction.Id = new Guid();
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                return transaction;
            }
            catch (Exception ex)
            {

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

                throw;
            }
        }
    }
}
