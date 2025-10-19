using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Purchase.Infrastructure.Configuration
{
    public class ExchangeRatesSettings
    {
        public string BaseUrl { get; set; }
        public int DefaultPageNumber { get; set; }
        public int DefaultPageSize { get; set; }
    }
}
