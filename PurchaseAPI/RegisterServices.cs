using Purchase.Core.Interfaces;
using Purchase.Infrastructure.Repositories;
using Purchase.Infrastructure.Services;

namespace PurchaseAPI
{
    public static class RegisterServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            // Services
            services.AddScoped<IExchangeService, ExchangeService>();
            services.AddScoped<ITransactionService, TransactionService>();

            return services;
        }
    }
}
