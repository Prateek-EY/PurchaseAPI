using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Purchase.Core.Entities;
using Purchase.Core.Interfaces;

namespace Purchase.Infrastructure.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _repository;

        public TransactionService(ITransactionRepository repository)
        {
            _repository = repository;
        }
        public async Task<PurchaseTransaction> CreateTransactionAsync(PurchaseTransaction transaction)
        {
            if (transaction.AmountUSD <= 0)
                throw new ArgumentException("Purchase amount must be positive");

            if (string.IsNullOrWhiteSpace(transaction.Description) || transaction.Description.Length > 50)
                throw new ArgumentException("Description is invalid or too long");

            return await _repository.AddAsync(transaction);
        }

        public async Task<List<PurchaseTransaction>> GetAllTransactionsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<PurchaseTransaction?> GetTransactionAsync(Guid id)
        {
            var transaction = await _repository.GetByIdAsync(id);
            if (transaction == null)
                throw new KeyNotFoundException($"Transaction with ID {id} not found");

            return transaction;
        }
    }
}
