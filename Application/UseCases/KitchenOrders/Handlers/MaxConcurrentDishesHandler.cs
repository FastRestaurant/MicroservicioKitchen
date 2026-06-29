using Application.Interfaces;
using Application.UseCases.KitchenOrders.Commands;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.UseCases.KitchenOrders.Handlers;

public sealed class MaxConcurrentDishesHandler : IMaxConcurrentDishesHandler
{
    private readonly IKitchenOrchestratorRepository _orchestratorRepository;
    private readonly IKitchenOrchestrator _orchestrator;

    public MaxConcurrentDishesHandler(
        IKitchenOrchestratorRepository orchestratorRepository,
        IKitchenOrchestrator orchestrator)
    {
        _orchestratorRepository = orchestratorRepository;
        _orchestrator = orchestrator;
    }

    public async Task ExecuteAsync(UpdateMaxConcurrentDishesCommand command, CancellationToken cancellationToken = default)
    {
        if (command.MaxConcurrentDishes <= 0)
        {
            throw new ValidationExceptions(new Dictionary<string, string[]>
            {
                { "maxConcurrentDishes", new[] { "La cantidad de preparaciones simultaneas debe ser mayor a 0." } }
            });
        }

        if (command.FactorMultiplierTime <= 0)
        {
            throw new ValidationExceptions(new Dictionary<string, string[]>
            {
                { "factorMultiplierTime", new[] { "El factor por cantidad debe ser mayor a 0." } }
            });
        }

        if (command.MaxQuantityTimeMultiplier < 1)
        {
            throw new ValidationExceptions(new Dictionary<string, string[]>
            {
                { "maxQuantityTimeMultiplier", new[] { "El tope por cantidad debe ser mayor o igual a 1." } }
            });
        }

        await _orchestratorRepository.UpdateConfigurationAsync(
            command.MaxConcurrentDishes,
            command.FactorMultiplierTime,
            command.MaxQuantityTimeMultiplier,
            cancellationToken);

        await _orchestrator.ScheduleAsync(cancellationToken);
    }
}
