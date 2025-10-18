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
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<PurchaseTransaction?> GetByIdAsync(Guid id)
        {
            return await _context.Transactions.FindAsync(id);
        }

        public async Task<List<PurchaseTransaction>> GetAllAsync()
        {
            return await _context.Transactions.ToListAsync();
        }

    }
}
