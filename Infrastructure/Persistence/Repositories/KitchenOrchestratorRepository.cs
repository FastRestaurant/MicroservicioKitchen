using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class KitchenOrchestratorRepository : IKitchenOrchestratorRepository
{
    private readonly ApplicationDbContext _context;

    public KitchenOrchestratorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetMaxConcurrentDishesAsync(CancellationToken cancellationToken = default)
    {
        var config = await GetConfigurationAsync(cancellationToken);
        return config.MaxConcurrentDishes;
    }

    public async Task<KitchenConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        var config = await _context.KitchenConfigurations.FirstOrDefaultAsync(cancellationToken);
        return config ?? new KitchenConfiguration
        {
            MaxConcurrentDishes = KitchenConfiguration.DefaultMaxConcurrentDishes,
            FactorMultiplierTime = KitchenConfiguration.DefaultFactorMultiplierTime,
            MaxQuantityTimeMultiplier = KitchenConfiguration.DefaultMaxQuantityTimeMultiplier
        };
    }

    public async Task UpdateConfigurationAsync(int maxConcurrentDishes, decimal factorMultiplierTime, decimal maxQuantityTimeMultiplier, CancellationToken cancellationToken = default)
    {
        var configuration = await _context.KitchenConfigurations.FirstOrDefaultAsync(cancellationToken);

        if (configuration == null)
            throw new NotFoundException("KitchenConfiguration", "default");

        configuration.MaxConcurrentDishes = maxConcurrentDishes;
        configuration.FactorMultiplierTime = factorMultiplierTime;
        configuration.MaxQuantityTimeMultiplier = maxQuantityTimeMultiplier;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
