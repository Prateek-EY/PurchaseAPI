using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Purchase.Core.Entities;

namespace Purchase.IntegrationTests
{
    public class TransactionRepositoryTests : IntegrationTestBase
    {

        [Fact]
        public async Task AddAsync_ShouldPersistTransaction()
        {
            var transaction = new PurchaseTransaction
            {
                Description = "Integration Test Add",
                TransactionDate = DateTime.UtcNow,
                AmountUSD = 100.25m
            };

            var result = await Repository.AddAsync(transaction);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("Integration Test Add", result.Description);

            // Verify it is in the database
            var fromDb = await Context.Transactions.FindAsync(result.Id);
            Assert.NotNull(fromDb);
            Assert.Equal(100.25m, fromDb.AmountUSD);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnTransaction_WhenExists()
        {
            var transaction = new PurchaseTransaction
            {
                Description = "Integration Test GetById",
                TransactionDate = DateTime.UtcNow,
                AmountUSD = 50.00m
            };
            Context.Transactions.Add(transaction);
            await Context.SaveChangesAsync();

            var result = await Repository.GetByIdAsync(transaction.Id);

            Assert.NotNull(result);
            Assert.Equal(transaction.Id, result.Id);
            Assert.Equal("Integration Test GetById", result.Description);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var result = await Repository.GetByIdAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllTransactions()
        {
            var transaction1 = new PurchaseTransaction
            {
                Description = "First",
                TransactionDate = DateTime.UtcNow,
                AmountUSD = 10.00m
            };
            var transaction2 = new PurchaseTransaction
            {
                Description = "Second",
                TransactionDate = DateTime.UtcNow,
                AmountUSD = 20.00m
            };
            Context.Transactions.AddRange(transaction1, transaction2);
            await Context.SaveChangesAsync();

            var result = await Repository.GetAllAsync();

            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
            Assert.Contains(result, t => t.Description == "First");
            Assert.Contains(result, t => t.Description == "Second");
        }

        [Fact]
        public async Task GetByIdsAsync_ShouldReturnMatchingTransactions()
        {
            var txn1 = new PurchaseTransaction
            {
                Description = "Batch1",
                TransactionDate = DateTime.UtcNow,
                AmountUSD = 10m
            };
            var txn2 = new PurchaseTransaction
            {
                Description = "Batch2",
                TransactionDate = DateTime.UtcNow,
                AmountUSD = 20m
            };
            var txn3 = new PurchaseTransaction
            {
                Description = "Batch3",
                TransactionDate = DateTime.UtcNow,
                AmountUSD = 30m
            };
            Context.Transactions.AddRange(txn1, txn2, txn3);
            await Context.SaveChangesAsync();

            var ids = new List<Guid> { txn1.Id, txn3.Id };
            var result = await Repository.GetByIdsAsync(ids);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.Description == "Batch1");
            Assert.Contains(result, t => t.Description == "Batch3");
            Assert.DoesNotContain(result, t => t.Description == "Batch2");
        }

        [Fact]
        public async Task GetByIdsAsync_ShouldReturnEmpty_WhenNoIdsMatch()
        {
            var txn = new PurchaseTransaction
            {
                Description = "NoMatch",
                TransactionDate = DateTime.UtcNow,
                AmountUSD = 10m
            };
            Context.Transactions.Add(txn);
            await Context.SaveChangesAsync();

            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var result = await Repository.GetByIdsAsync(ids);

            Assert.Empty(result);
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenDescriptionTooLong()
        {
            var txn = new PurchaseTransaction
            {
                Description = new string('x', 100),
                TransactionDate = DateTime.UtcNow,
                AmountUSD = 10m
            };

            await Assert.ThrowsAsync<DbUpdateException>(() => Repository.AddAsync(txn));
            Context.Entry(txn).State = EntityState.Detached;
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenAmountIsNegative()
        {
            var txn = new PurchaseTransaction
            {
                Description = "NegativeAmount",
                TransactionDate = DateTime.UtcNow,
                AmountUSD = -5m
            };

            await Assert.ThrowsAsync<Exception>(() => Repository.AddAsync(txn));
            Context.Entry(txn).State = EntityState.Detached;
        }



    }
}
