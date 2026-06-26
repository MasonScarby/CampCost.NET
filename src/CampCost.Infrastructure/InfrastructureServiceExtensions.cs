using CampCost.Core.Interfaces;
using CampCost.Core.Services;
using CampCost.Infrastructure.Data;
using CampCost.Infrastructure.Services;
using Going.Plaid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CampCost.Infrastructure;

/// <summary>
/// Single call from Program.cs wires up the entire Infrastructure layer.
/// The Api project never needs to reference Going.Plaid or EF Core directly.
/// </summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        // Database
        services.AddDbContext<CampCostDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        // Plaid client — uses Going.Plaid's built-in DI extension
        // Reads ClientId, Secret, Environment from config section "Plaid"
        services.AddHttpClient();
        services.Configure<PlaidOptions>(config.GetSection("Plaid"));
        services.AddSingleton<PlaidClient>();

        // Core stateless services
        services.AddSingleton<TransactionCategorizationService>();
        services.AddSingleton<TransactionMatchingService>();
        services.AddSingleton<BudgetCalculationService>();

        // Scoped services (one per HTTP request)
        services.AddScoped<IPlaidService, PlaidService>();
        services.AddScoped<ITransactionSyncService, TransactionSyncService>();

        return services;
    }
}
