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
        // Map to exact Supabase table names
        modelBuilder.Entity<Trip>().ToTable("trips");
        modelBuilder.Entity<Expense>().ToTable("expenses");
        modelBuilder.Entity<PlaidConnection>().ToTable("plaid_connections");

        // trips — map C# PascalCase properties to snake_case DB columns
        modelBuilder.Entity<Trip>(e =>
        {
            e.Property(t => t.Id).HasColumnName("id");
            e.Property(t => t.UserId).HasColumnName("user_id");
            e.Property(t => t.Name).HasColumnName("name");
            e.Property(t => t.Destination).HasColumnName("destination");
            e.Property(t => t.StartDate).HasColumnName("start_date");
            e.Property(t => t.EndDate).HasColumnName("end_date");
            e.Property(t => t.TotalBudget).HasColumnName("total_budget");
            e.Property(t => t.Status).HasColumnName("status");
            e.Property(t => t.CreatedAt).HasColumnName("created_at");
        });

        // expenses — column names must match Supabase schema exactly
        modelBuilder.Entity<Expense>(e =>
        {
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.TripId).HasColumnName("trip_id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.PlaidTransactionId).HasColumnName("plaid_transaction_id");
            e.Property(x => x.MerchantName).HasColumnName("merchant_name");
            e.Property(x => x.Amount).HasColumnName("amount");
            e.Property(x => x.Category).HasColumnName("category");
            e.Property(x => x.ExpenseDate).HasColumnName("expense_date");
            e.Property(x => x.Source).HasColumnName("source");
            e.Property(x => x.Reviewed).HasColumnName("reviewed");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => x.PlaidTransactionId).IsUnique();
        });

        // plaid_connections
        modelBuilder.Entity<PlaidConnection>(e =>
        {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.UserId).HasColumnName("user_id");
            e.Property(p => p.AccessToken).HasColumnName("access_token");
            e.Property(p => p.ItemId).HasColumnName("item_id");
            e.Property(p => p.InstitutionName).HasColumnName("institution_name");
            e.Property(p => p.LastSyncAt).HasColumnName("last_sync_at");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
        });
    }
}
