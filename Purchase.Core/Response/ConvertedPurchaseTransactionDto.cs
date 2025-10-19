using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Purchase.Core.Response
{
    public class ConvertedPurchaseTransactionDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public decimal AmountUSD { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal ConvertedAmount { get; set; }
        public string TargetCurrency { get; set; } = null!;
    }
}
