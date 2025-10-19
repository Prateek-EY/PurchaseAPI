using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Purchase.Core.Entities;
using Purchase.Core.Interfaces;
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
            var transaction = new PurchaseTransaction
            {
                Id = Guid.NewGuid(),
                Description = "Test",
                TransactionDate = DateTime.UtcNow,
                AmountUSD = 10.00m
            };
            _repoMock.Setup(r => r.AddAsync(transaction)).ReturnsAsync(transaction);

            var result = await _service.CreateTransactionAsync(transaction);

            Assert.Equal(transaction, result);
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
            var transaction = new PurchaseTransaction
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
            var transaction = new PurchaseTransaction
            {
                Id = Guid.NewGuid(),
                Description = "Test",
                TransactionDate = DateTime.UtcNow,
                AmountUSD = -5
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransactionAsync(transaction));
        }
    }
}
