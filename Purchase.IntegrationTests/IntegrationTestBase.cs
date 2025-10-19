using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Purchase.Infrastructure.Configuration;
using Purchase.Infrastructure.Data;
using Purchase.Infrastructure.Repositories;

namespace Purchase.IntegrationTests
{
    public class IntegrationTestBase : IDisposable
    {
        protected readonly AppDbContext Context;
        protected readonly TransactionRepository Repository;

        protected IntegrationTestBase()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var postgresSettings = new PostgreSettings();
            configuration.GetSection("PostgreSettings").Bind(postgresSettings);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(postgresSettings.BuildConnectionString())
                .Options;

            Context = new AppDbContext(options);
            Context.Database.Migrate();
            Repository = new TransactionRepository(Context);

        }

        public void Dispose()
        {
            Context.Transactions.RemoveRange(Context.Transactions);
            Context.SaveChanges();
            Context.Dispose();
        }
    }
}
