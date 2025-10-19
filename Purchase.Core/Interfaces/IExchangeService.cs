using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Purchase.Core.Response;

namespace Purchase.Core.Interfaces
{
    public interface IExchangeService
    {
        Task<List<ExchangeRateRecord>> GetExchangeRatesAsync(string countryCurrencyDesc, DateTime from, DateTime to);
    }
}
