using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Purchase.Core.Entities
{
    public class ExchangeRateApiResponse
    {
        [JsonPropertyName("data")]
        public List<ExchangeRateData> Data { get; set; }
        [JsonPropertyName("links")]
        public ApiLinks Links { get; set; }
    }
    public class ExchangeRateData
    {
        [JsonPropertyName("country_currency_desc")]
        public string CountryCurrencyDesc { get; set; }
        [JsonPropertyName("exchange_rate")]
        public string ExchangeRate { get; set; }
        [JsonPropertyName("record_date")]
        public string RecordDate { get; set; }
    }

    public class ApiLinks
    {
        [JsonPropertyName("next")]
        public string? Next { get; set; }
    }
}
