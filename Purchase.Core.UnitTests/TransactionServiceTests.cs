using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Purchase.Core.Entities;
using Purchase.Core.Interfaces;
using Purchase.Core.Request;
using Purchase.Core.Response;
using Purchase.Infrastructure.Services;

namespace Purchase.Core.UnitTests
{
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _repoMock;
        private readonly Mock<IExchangeService> _exchangeServiceMock;
        private readonly Mock<ILogger<TransactionService>> _loggerMock;
        private readonly TransactionService _service;

        public TransactionServiceTests()
        {
            _repoMock = new Mock<ITransactionRepository>();
            _exchangeServiceMock = new Mock<IExchangeService>();
            _loggerMock = new Mock<ILogger<TransactionService>>();
            _service = new TransactionService(_repoMock.Object, _exchangeServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateTransactionAsync_ValidTransaction_ReturnsTransaction()
        {
            var transactionRequest = new PurchaseTransactionRequest
            {
                Id = Guid.NewGuid(),
                Description = "Test",
                TransactionDate = DateTime.UtcNow,
                AmountUSD = 10.00m
            };

            var transaction = new PurchaseTransaction
            {
                Id = transactionRequest.Id,
                Description = transactionRequest.Description,
                TransactionDate = transactionRequest.TransactionDate,
                AmountUSD = transactionRequest.AmountUSD
            };

            _repoMock.Setup(r => r.AddAsync(It.IsAny<PurchaseTransactionRequest>())).ReturnsAsync(transaction);

            var result = await _service.CreateTransactionAsync(transactionRequest);

            Assert.Equal(transaction.Id, result.Id);
            Assert.Equal(transaction.Description, result.Description);
            Assert.Equal(transaction.TransactionDate, result.TransactionDate);
            Assert.Equal(transaction.AmountUSD, result.AmountUSD);
        }

        [Fact]
        public async Task GetAllTransactionsAsync_ReturnsList()
        {
            var transactions = new List<PurchaseTransaction>
                {
                    new PurchaseTransaction { Id = Guid.NewGuid(), Description = "A", TransactionDate = DateTime.UtcNow, AmountUSD = 1 }
                };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(transactions);

            var result = await _service.GetAllTransactionsAsync();

            Assert.Equal(transactions, result);
        }

        [Fact]
        public async Task GetTransactionAsync_ExistingId_ReturnsTransaction()
        {
            var id = Guid.NewGuid();
            var transaction = new PurchaseTransaction { Id = id, Description = "A", TransactionDate = DateTime.UtcNow, AmountUSD = 1 };
            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(transaction);

            var result = await _service.GetTransactionAsync(id);

            Assert.Equal(transaction, result);
        }

        [Fact]
        public async Task GetTransactionAsync_NonExistingId_ThrowsKeyNotFoundException()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((PurchaseTransaction?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetTransactionAsync(id));
        }

        [Fact]
        public async Task CreateTransactionAsync_InvalidDescription_ThrowsArgumentException()
        {
            var transaction = new PurchaseTransactionRequest
            {
                Id = Guid.NewGuid(),
                Description = "This description is way too long for the allowed fifty character limit!",
                TransactionDate = DateTime.UtcNow,
                AmountUSD = 10.00m
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransactionAsync(transaction));
        }

        [Fact]
        public async Task CreateTransactionAsync_InvalidAmount_ThrowsArgumentException()
        {
            var transaction = new PurchaseTransactionRequest
            {
                Id = Guid.NewGuid(),
                Description = "Test",
                TransactionDate = DateTime.UtcNow,
                AmountUSD = -5
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransactionAsync(transaction));
        }

        [Fact]
        public async Task GetTransactionsInCurrencyAsync_ReturnsWarning_WhenNoExchangeRateFound()
        {
            var txn = new PurchaseTransaction
            {
                Id = Guid.NewGuid(),
                Description = "Test",
                AmountUSD = 100,
                TransactionDate = DateTime.Today
            };
            _repoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>())).ReturnsAsync(new List<PurchaseTransaction> { txn });
            _exchangeServiceMock.Setup(e => e.GetExchangeRatesAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ExchangeRateRecord>());

            var result = await _service.GetTransactionsInCurrencyAsync(new List<Guid> { txn.Id }, "EUR");

            Assert.Single(result);
            Assert.Equal(0, result[0].ExchangeRate);
            Assert.Equal(0, result[0].ConvertedAmount);
            Assert.Contains("Cannot be converted", result[0].Description);
        }

        [Fact]
        public async Task GetTransactionsInCurrencyAsync_UsesMostRecentRateWithin6Months()
        {
            var txn = new PurchaseTransaction
            {
                Id = Guid.NewGuid(),
                Description = "Test",
                AmountUSD = 100,
                TransactionDate = new DateTime(2025, 10, 20)
            };
            var rate = new ExchangeRateRecord
            {
                CountryCurrencyDesc = "EUR",
                Rate = 2.0m,
                Date = new DateTime(2025, 10, 19)
            };
            _repoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>())).ReturnsAsync(new List<PurchaseTransaction> { txn });
            _exchangeServiceMock.Setup(e => e.GetExchangeRatesAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ExchangeRateRecord> { rate });

            var result = await _service.GetTransactionsInCurrencyAsync(new List<Guid> { txn.Id }, "EUR");

            Assert.Single(result);
            Assert.Equal(rate.Rate, result[0].ExchangeRate);
            Assert.Equal(200, result[0].ConvertedAmount);
        }

        [Fact]
        public async Task GetAllTransactionsAsync_CallsRepository()
        {
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PurchaseTransaction>());

            var result = await _service.GetAllTransactionsAsync();

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            Assert.Empty(result);
        }
    }
}
