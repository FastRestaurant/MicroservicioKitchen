using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases.KitchenOrders.Handlers;

public sealed class GetKitchenConfigurationHandler : IGetKitchenConfigurationHandler
{
    private readonly IKitchenOrchestratorRepository _orchestratorRepository;

    public GetKitchenConfigurationHandler(IKitchenOrchestratorRepository orchestratorRepository)
    {
        _orchestratorRepository = orchestratorRepository;
    }

    public async Task<KitchenConfigurationDto> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var configuration = await _orchestratorRepository.GetConfigurationAsync(cancellationToken);
        return new KitchenConfigurationDto
        {
            MaxConcurrentDishes = configuration.MaxConcurrentDishes,
            FactorMultiplierTime = configuration.FactorMultiplierTime,
            MaxQuantityTimeMultiplier = configuration.MaxQuantityTimeMultiplier
        };
    }
}
