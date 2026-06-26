using CampCost.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampCost.Infrastructure.Data;

public class CampCostDbContext : DbContext
{
    public CampCostDbContext(DbContextOptions<CampCostDbContext> options) : base(options) { }

    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<PlaidConnection> PlaidConnections => Set<PlaidConnection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Trip>().ToTable("trips");
        modelBuilder.Entity<Expense>().ToTable("expenses");
        modelBuilder.Entity<PlaidConnection>().ToTable("plaid_connections");

        modelBuilder.Entity<Expense>()
            .HasIndex(e => e.PlaidTransactionId)
            .IsUnique();
    }
}
