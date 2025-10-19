using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Purchase.Core.Entities;
using Purchase.Core.Interfaces;
using Purchase.Core.Response;

namespace Purchase.Infrastructure.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _repository;

        private readonly IExchangeService _exchangeService;

        private readonly ILogger<TransactionService> _logger;

        public TransactionService(ITransactionRepository repository, IExchangeService exchangeService, ILogger<TransactionService> logger)
        {
            _repository = repository;
            _exchangeService = exchangeService;
            _logger = logger;
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

        public async Task<List<ConvertedPurchaseTransactionDto>> GetTransactionsInCurrencyAsync(List<Guid> transactionIds, string targetCurrency)
        {
            _logger.LogInformation("Starting currency conversion for {Count} transactions to {Currency}.", transactionIds.Count, targetCurrency);
            var results = new ConcurrentBag<ConvertedPurchaseTransactionDto>();

            var allTransactions = new List<PurchaseTransaction>();
            foreach (var batch in transactionIds.Chunk(1000))
            {
                _logger.LogDebug("Fetching batch of {BatchSize} transactions from repository.", batch.Count());
                var batchTxns = await _repository.GetByIdsAsync(batch.ToList());
                allTransactions.AddRange(batchTxns);
            }

            if (!allTransactions.Any())
            {
                _logger.LogWarning("No transactions found for the provided IDs.");
                return results.ToList();
            }


            var earliestDate = allTransactions.Min(t => t.TransactionDate).AddMonths(-6);
            var latestDate = allTransactions.Max(t => t.TransactionDate);

            _logger.LogInformation("Fetching exchange rates for {Currency} from {From} to {To}.", targetCurrency, earliestDate.ToShortDateString(), latestDate.ToShortDateString());


            var exchangeRates = await _exchangeService.GetExchangeRatesAsync(targetCurrency, earliestDate, latestDate);

      
            await Parallel.ForEachAsync(allTransactions, new ParallelOptions { MaxDegreeOfParallelism = 10 }, (txn, ct) =>
            {
                try
                {
                    var rateRecord = exchangeRates
                                .Where(r => r.Date <= txn.TransactionDate && r.Date >= txn.TransactionDate.AddMonths(-6))
                                .OrderByDescending(r => r.Date)
                                .FirstOrDefault();

                    if (rateRecord == null)
                    {
                        _logger.LogWarning("No exchange rate found for transaction {TransactionId} on {Date} to {Currency}.", txn.Id, txn.TransactionDate, targetCurrency);

                        results.Add(new ConvertedPurchaseTransactionDto
                        {
                            Id = txn.Id,
                            Description = txn.Description + " (Cannot be converted to the target currency)",
                            TransactionDate = txn.TransactionDate,
                            AmountUSD = txn.AmountUSD,
                            ExchangeRate = 0,
                            ConvertedAmount = 0,
                            TargetCurrency = targetCurrency
                        });
                    }
                    else
                    {
                        results.Add(new ConvertedPurchaseTransactionDto
                        {
                            Id = txn.Id,
                            Description = txn.Description,
                            TransactionDate = txn.TransactionDate,
                            AmountUSD = txn.AmountUSD,
                            ExchangeRate = rateRecord.Rate,
                            ConvertedAmount = Math.Round(txn.AmountUSD * rateRecord.Rate, 2),
                            TargetCurrency = targetCurrency
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error converting transaction {TransactionId} to {Currency}.", txn.Id, targetCurrency);
                 
                }

                return ValueTask.CompletedTask;
            });
            _logger.LogInformation("Completed currency conversion for {Count} transactions to {Currency}.", results.Count, targetCurrency);

            return results.ToList();
        }
    }
}
