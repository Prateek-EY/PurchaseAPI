using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Purchase.Core.Entities;
using Purchase.Core.Interfaces;
using Purchase.Core.Request;

namespace PurchaseAPI.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpPost]
        [Route("create/transaction")]
        public async Task<IActionResult> Create([FromBody] PurchaseTransactionRequest transaction)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
            return CreatedAtAction(nameof(GetById), new { id = createdTransaction.Id }, createdTransaction);
        }

        [HttpGet]
        [Route("/transaction/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var transaction = await _transactionService.GetTransactionAsync(id);
            return Ok(transaction);
        }

        [HttpPost]
        [Route("/currencyconvertor")]
        public async Task<IActionResult> GetTransactionsInCurrency([FromBody] TransactionCurrencyRequest request)
        {
            if (request.TransactionIds == null || !request.TransactionIds.Any())
                return BadRequest("TransactionIds cannot be empty.");

            if (string.IsNullOrWhiteSpace(request.TargetCurrency))
                return BadRequest("TargetCurrency is required.");

            try
            {
            
                _logger.LogInformation("Fetching transactions in currency {Currency} for {Count} transactions. CorrelationId={CorrelationId}",
                    request.TargetCurrency, request.TransactionIds.Count);

                var result = await _transactionService.GetTransactionsInCurrencyAsync(
                    request.TransactionIds,
                    request.TargetCurrency
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transactions. CorrelationId={CorrelationId}", HttpContext.TraceIdentifier);
                return StatusCode(500, new { error = ex.Message, correlationId = HttpContext.TraceIdentifier });
            }
        }
    }
}
