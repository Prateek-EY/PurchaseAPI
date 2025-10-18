using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Purchase.Core.Entities;

namespace Purchase.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet for purchase transactions
        public DbSet<PurchaseTransaction> Transactions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PurchaseTransaction>()
                .Property(t => t.Description)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<PurchaseTransaction>()
                .Property(t => t.AmountUSD)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        }
    }
}
