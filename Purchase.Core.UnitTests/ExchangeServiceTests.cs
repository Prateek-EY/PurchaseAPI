using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq.Protected;
using Moq;
using Purchase.Core.Entities;
using Purchase.Core.Response;
using Purchase.Core;
using Purchase.Infrastructure.Configuration;
using Purchase.Infrastructure.Services;

namespace Purchase.UnitTests
{
    public class ExchangeServiceTests
    {
        private ExchangeService CreateService(HttpResponseMessage response, ExchangeRatesSettings settings = null)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(handlerMock.Object));

            var options = Options.Create(settings ?? new ExchangeRatesSettings
            {
                BaseUrl = "http://test/api",
                DefaultPageNumber = 1,
                DefaultPageSize = 10
            });

            var loggerMock = new Mock<ILogger<ExchangeService>>();

            return new ExchangeService(httpClientFactoryMock.Object, options, loggerMock.Object);
        }

        [Fact]
        public async Task GetExchangeRatesAsync_ReturnsRates_WhenApiReturnsData()
        {
            var apiResponse = new ExchangeRateApiResponse
            {
                Data = new List<ExchangeRateData>
                {
                    new ExchangeRateData
                    {
                        CountryCurrencyDesc = "USD", ExchangeRate = "1.23", RecordDate = new DateTime(2025, 10, 20).ToString()
                    }
                },
                Links = new ApiLinks { Next = null }
            };

            var json = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var service = CreateService(response);

            var result = await service.GetExchangeRatesAsync("USD", DateTime.Today, DateTime.Today);

            Assert.Single(result);
            Assert.Equal("USD", result[0].CountryCurrencyDesc);
            Assert.Equal(1.23m, result[0].Rate);
        }

        [Fact]
        public async Task GetExchangeRatesAsync_ReturnsEmpty_WhenApiReturnsNoData()
        {
            var apiResponse = new ExchangeRateApiResponse
            {
                Data = new List<ExchangeRateData>(),
                Links = new ApiLinks { Next = null }
            };

            var json = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var service = CreateService(response);

            var result = await service.GetExchangeRatesAsync("USD", DateTime.Today, DateTime.Today);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetExchangeRatesAsync_ThrowsExternalApiException_OnNonSuccessStatusCode()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

            var service = CreateService(response);

            await Assert.ThrowsAsync<ExternalApiException>(() =>
                service.GetExchangeRatesAsync("USD", DateTime.Today, DateTime.Today));
        }

        
    }
}

