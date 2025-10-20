
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Purchase.Core.Entities;
using Purchase.Core.Interfaces;
using Purchase.Core.Request;
using Purchase.Core.Response;
using PurchaseAPI.Controllers;

namespace Purchase.UnitTests
{
    public class TransactionControllerTests
    {
        private readonly Mock<ITransactionService> _serviceMock = new();
        private readonly Mock<ILogger<TransactionController>> _loggerMock = new();

        private TransactionController CreateController()
        {
            return new TransactionController(_serviceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenModelStateInvalid()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("Description", "Required");

            var result = await controller.Create(new PurchaseTransactionRequest());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenValid()
        {
            var request = new PurchaseTransactionRequest
            {
                Description = "Test",
                TransactionDate = DateTime.UtcNow,
                AmountUSD = 10
            };
            var created = new PurchaseTransaction
            {
                Id = Guid.NewGuid(),
                Description = request.Description,
                TransactionDate = request.TransactionDate,
                AmountUSD = request.AmountUSD
            };
            _serviceMock.Setup(s => s.CreateTransactionAsync(request)).ReturnsAsync(created);

            var controller = CreateController();
            var result = await controller.Create(request);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(controller.GetById), createdResult.ActionName);
            Assert.Equal(created.Id, ((PurchaseTransaction)createdResult.Value).Id);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenFound()
        {
            var id = Guid.NewGuid();
            var txn = new PurchaseTransaction { Id = id, Description = "Test", TransactionDate = DateTime.UtcNow, AmountUSD = 10 };
            _serviceMock.Setup(s => s.GetTransactionAsync(id)).ReturnsAsync(txn);

            var controller = CreateController();
            var result = await controller.GetById(id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(txn, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenNotFound()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetTransactionAsync(id)).ReturnsAsync((PurchaseTransaction?)null);

            var controller = CreateController();
            var result = await controller.GetById(id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        [Fact]
        public async Task GetTransactionsInCurrency_ReturnsBadRequest_WhenNoTransactionIds()
        {
            var controller = CreateController();
            var request = new TransactionCurrencyRequest { TransactionIds = new List<Guid>(), TargetCurrency = "EUR" };

            var result = await controller.GetTransactionsInCurrency(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("TransactionIds cannot be empty.", badRequest.Value);
        }

        [Fact]
        public async Task GetTransactionsInCurrency_ReturnsBadRequest_WhenNoTargetCurrency()
        {
            var controller = CreateController();
            var request = new TransactionCurrencyRequest { TransactionIds = new List<Guid> { Guid.NewGuid() }, TargetCurrency = "" };

            var result = await controller.GetTransactionsInCurrency(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("TargetCurrency is required.", badRequest.Value);
        }

        [Fact]
        public async Task GetTransactionsInCurrency_ReturnsOk_WhenValid()
        {
            var ids = new List<Guid> { Guid.NewGuid() };
            var request = new TransactionCurrencyRequest { TransactionIds = ids, TargetCurrency = "EUR" };
            var dtos = new List<ConvertedPurchaseTransactionDto>
        {
            new ConvertedPurchaseTransactionDto
            {
                Id = ids[0],
                Description = "Test",
                TransactionDate = DateTime.UtcNow,
                AmountUSD = 10,
                ExchangeRate = 1.1m,
                ConvertedAmount = 11,
                TargetCurrency = "EUR"
            }
        };
            _serviceMock.Setup(s => s.GetTransactionsInCurrencyAsync(ids, "EUR")).ReturnsAsync(dtos);

            var controller = CreateController();
            var result = await controller.GetTransactionsInCurrency(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dtos, okResult.Value);
        }

    }
}

