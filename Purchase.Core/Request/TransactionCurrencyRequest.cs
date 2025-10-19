using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Purchase.Core.Request
{
    public class TransactionCurrencyRequest
    {
        public List<Guid> TransactionIds { get; set; } = new();
        public string TargetCurrency { get; set; } = null!;
    }
}
