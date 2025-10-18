using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Purchase.Core.Entities;
using Purchase.Core.Interfaces;

namespace PurchaseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PurchaseTransaction transaction)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
            return CreatedAtAction(nameof(GetById), new { id = createdTransaction.Id }, createdTransaction);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var transaction = await _transactionService.GetTransactionAsync(id);
            return Ok(transaction);
        }

        //[HttpGet("{id:guid}/convert/{currencyCode}")]
        //public async Task<IActionResult> GetByIdInCurrency(Guid id, string currencyCode)
        //{
        //    // This method assumes your service calls the Treasury API for conversion
        //    var transactionInCurrency = await _transactionService.GetTransactionInCurrencyAsync(id, currencyCode.ToUpper());
        //    return Ok(transactionInCurrency);
        //}
    }
}
