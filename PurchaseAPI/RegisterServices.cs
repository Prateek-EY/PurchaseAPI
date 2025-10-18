using Purchase.Core.Interfaces;
using Purchase.Infrastructure.Repositories;

namespace PurchaseAPI
{
    public static class RegisterServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            // Services
            //services.AddScoped<ITransactionService, TransactionService>();

            return services;
        }
    }
}
