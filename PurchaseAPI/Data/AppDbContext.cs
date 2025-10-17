using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using PurchaseAPI.Model;

namespace PurchaseAPI.Data
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
