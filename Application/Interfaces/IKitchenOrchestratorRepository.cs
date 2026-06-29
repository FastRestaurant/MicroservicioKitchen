using Domain.Entities;

namespace Application.Interfaces;

public interface IKitchenOrchestratorRepository
{
    Task<KitchenConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);
    Task<int> GetMaxConcurrentDishesAsync(CancellationToken cancellationToken = default);
    Task UpdateConfigurationAsync(int maxConcurrentDishes, decimal factorMultiplierTime, decimal maxQuantityTimeMultiplier, CancellationToken cancellationToken = default);
}
