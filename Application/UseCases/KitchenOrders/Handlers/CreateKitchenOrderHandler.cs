using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.KitchenOrders.Commands;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.UseCases.KitchenOrders.Handlers;

public sealed class CreateKitchenOrderHandler : ICreateKitchenOrderHandler
{
    private readonly IKitchenOrderRepository _repository;
    private readonly IKitchenOrchestrator _orchestrator;
    private readonly IKitchenOrchestratorRepository _orchestratorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateKitchenOrderHandler(
        IKitchenOrderRepository repository,
        IKitchenOrchestrator orchestrator,
        IKitchenOrchestratorRepository orchestratorRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _orchestrator = orchestrator;
        _orchestratorRepository = orchestratorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateKitchenOrderResponseDto> CreateKitchenOrder(CreateKitchenOrderCommand command, CancellationToken cancellationToken = default)
    {
        Validate(command);
        var configuration = await _orchestratorRepository.GetConfigurationAsync(cancellationToken);

        var existing = await _repository.GetByOrderIdWithItemsAsync(command.OrderId, cancellationToken);
        if (existing is not null)
        {
            var existingOrderItemIds = existing.Items.Select(item => item.OrderItemId).ToHashSet();
            var newItems = command.Items
                .Where(item => !existingOrderItemIds.Contains(item.OrderItemId))
                .ToList();

            if (newItems.Count > 0)
            {
                var itemsToAdd = new List<KitchenOrderItem>();
                foreach (var itemDto in newItems)
                {
                    var item = BuildItem(itemDto, configuration);
                    existing.AddItem(item);
                    itemsToAdd.Add(item);
                }

                await _unitOfWork.ExecuteAsync(async () =>
                {
                    await _repository.AddItemsAsync(itemsToAdd, cancellationToken);
                    await _orchestrator.EnqueueOrderWithoutNotificationAsync(existing.Id, cancellationToken);
                }, cancellationToken);

                await _orchestrator.NotifyQueueChangedAsync(cancellationToken);
            }

            return new CreateKitchenOrderResponseDto
            {
                Success = true,
                Message = newItems.Count == 0 ? "La orden ya estaba registrada en cocina." : "Items agregados a cocina.",
                KitchenOrderId = existing.Id
            };
        }

        var order = BuildOrder(command, configuration);

        var savedOrder = await _unitOfWork.ExecuteAsync(async () =>
        {
            var created = await _repository.CreateAsync(order, cancellationToken);
            await _orchestrator.EnqueueOrderWithoutNotificationAsync(created.Id, cancellationToken);
            return created;
        }, cancellationToken);

        await _orchestrator.NotifyQueueChangedAsync(cancellationToken);

        return new CreateKitchenOrderResponseDto
        {
            Success = true,
            KitchenOrderId = savedOrder.Id
        };
    }

    private static void Validate(CreateKitchenOrderCommand command)
    {
        var errors = new Dictionary<string, string[]>();

        if (command.OrderId == Guid.Empty)
            errors["orderId"] = new[] { "El OrderId es obligatorio." };

        if (command.TableNumber <= 0)
            errors["tableNumber"] = new[] { "El numero de mesa debe ser mayor a 0." };

        if (command.Items is null || command.Items.Count == 0)
        {
            errors["items"] = new[] { "La orden debe contener al menos un item." };
        }
        else
        {
            for (var i = 0; i < command.Items.Count; i++)
            {
                var item = command.Items[i];
                var messages = new List<string>();

                if (item.ProductId == Guid.Empty)
                    messages.Add("El ProductId es obligatorio.");
                if (string.IsNullOrWhiteSpace(item.ProductName))
                    messages.Add("El nombre del producto es obligatorio.");
                if (item.DurationMinutes <= 0)
                    messages.Add("El tiempo estimado debe ser mayor a 0 minutos.");
                if (item.Quantity <= 0)
                    messages.Add("La cantidad debe ser mayor a 0.");

                if (messages.Count > 0)
                    errors[$"items[{i}]"] = messages.ToArray();
            }
        }

        if (errors.Count > 0)
            throw new ValidationExceptions(errors);
    }

    private static KitchenOrder BuildOrder(CreateKitchenOrderCommand command, KitchenConfiguration configuration)
    {
        var order = KitchenOrder.Create(command.OrderId, command.TableId, command.TableNumber, command.WaiterId);

        foreach (var itemDto in command.Items)
            order.AddItem(BuildItem(itemDto, configuration));

        return order;
    }

    private static KitchenOrderItem BuildItem(CreateKitchenOrderItemDto itemDto, KitchenConfiguration configuration)
    {
        return KitchenOrderItem.Create(
            itemDto.OrderItemId,
            itemDto.ProductId,
            itemDto.ProductName,
            itemDto.Quantity,
            itemDto.DurationMinutes,
            configuration.FactorMultiplierTime,
            configuration.MaxQuantityTimeMultiplier,
            itemDto.Notes);
    }
}
