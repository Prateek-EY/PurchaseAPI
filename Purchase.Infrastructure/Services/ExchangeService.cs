using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Purchase.Core;
using Purchase.Core.Entities;
using Purchase.Core.Interfaces;
using Purchase.Core.Response;
using Purchase.Infrastructure.Configuration;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Purchase.Infrastructure.Services
{
    public class ExchangeService : IExchangeService
    {
        private readonly HttpClient client;
        private readonly ExchangeRatesSettings _settings;
        private readonly ILogger<ExchangeService> _logger;

        public ExchangeService(IHttpClientFactory httpClientFactory, IOptions<ExchangeRatesSettings> options, ILogger<ExchangeService> logger)
        {
            client = httpClientFactory.CreateClient();
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<List<ExchangeRateRecord>> GetExchangeRatesAsync(string countryCurrencyDesc, DateTime from, DateTime to)
        {
            var allRates = new List<ExchangeRateRecord>();
            var page = _settings.DefaultPageNumber;
            var pageSize = _settings.DefaultPageSize;
            bool hasNext = true;

            _logger.LogInformation("Fetching exchange rates for {Currency} from {FromDate} to {ToDate}",
           countryCurrencyDesc, from.ToShortDateString(), to.ToShortDateString());

            while (hasNext)
            {
                try
                {
                    var queryParams = new Dictionary<string, string>
                    {
                        ["fields"] = "country_currency_desc,exchange_rate,record_date",
                        ["filter"] = $"country_currency_desc:eq:{countryCurrencyDesc},record_date:gte:{from:yyyy-MM-dd},record_date:lte:{to:yyyy-MM-dd}",
                        ["page[number]"] = page.ToString(),
                        ["page[size]"] = pageSize.ToString()
                    };

                    var url = QueryHelpers.AddQueryString(_settings.BaseUrl, queryParams);

                    _logger.LogDebug("Calling Treasury API: {Url}", url);

                    var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Treasury API call failed with status code {StatusCode} for {Url}", response.StatusCode, url);

                        throw new ExternalApiException(
                            $"Treasury API call failed: {response.StatusCode}",
                                (int)response.StatusCode
                        );
                    }

                   
                    
                       var apiResponse = await JsonSerializer.DeserializeAsync<ExchangeRateApiResponse>(
                            await response.Content.ReadAsStreamAsync(),
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );
                   
                    

                    if (apiResponse?.Data == null || !apiResponse.Data.Any())
                    {
                        _logger.LogWarning("No exchange rate data returned for {Currency} on page {Page}", countryCurrencyDesc, page);
                        break;
                    }

                    foreach (var r in apiResponse.Data)
                    {
                        if (!DateTime.TryParseExact(r.RecordDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var recordDate))
                            continue;

                        if (!decimal.TryParse(r.ExchangeRate, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate))
                            continue;

                        allRates.Add(new ExchangeRateRecord
                        {
                            CountryCurrencyDesc = r.CountryCurrencyDesc,
                            Rate = rate,
                            Date = recordDate
                        });
                    }

                    hasNext = !string.IsNullOrEmpty(apiResponse.Links?.Next);
                    
                    page++;
                }
                catch (Exception ex)
                {

                    throw;
                }
            }

            return allRates;
        }
    }
}
