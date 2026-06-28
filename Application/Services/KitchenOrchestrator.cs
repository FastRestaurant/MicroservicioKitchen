using Application.DTOs;
using Application.Interfaces;
using Application.Realtime;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Services;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public sealed class KitchenOrchestrator : IKitchenOrchestrator
{
    private static readonly SemaphoreSlim ScheduleGate = new(1, 1);

    private readonly IKitchenOrderRepository _repository;
    private readonly IKitchenOrderItemRepository _itemRepository;
    private readonly IKitchenOrchestratorRepository _orchestratorRepository;
    private readonly IOrderServiceClient _orderServiceClient;
    private readonly KitchenSchedulingPolicy _schedulingPolicy;
    private readonly IKitchenNotifier _kitchenNotifier;
    private readonly ILogger<KitchenOrchestrator> _logger;

    public KitchenOrchestrator(
        IKitchenOrderRepository repository,
        IKitchenOrchestratorRepository orchestratorRepository,
        IKitchenOrderItemRepository itemRepository,
        IOrderServiceClient orderServiceClient,
        KitchenSchedulingPolicy schedulingPolicy,
        IKitchenNotifier kitchenNotifier,
        ILogger<KitchenOrchestrator> logger)
    {
        _repository = repository;
        _orchestratorRepository = orchestratorRepository;
        _itemRepository = itemRepository;
        _orderServiceClient = orderServiceClient;
        _schedulingPolicy = schedulingPolicy;
        _kitchenNotifier = kitchenNotifier;
        _logger = logger;
    }

    public async Task<List<KitchenQueueItemResponse>> GetItemsFromQueueAsync(CancellationToken cancellationToken = default)
    {
        var items = await _itemRepository.GetItemsReadyToCookAsync(cancellationToken);
        return items.Select(MapToQueueItem).ToList();
    }

    public async Task<List<KitchenQueueItemResponse>> GetWaitingItemsAsync(CancellationToken cancellationToken = default)
    {
        var waiting = await _itemRepository.GetItemsToWaitingAsync(cancellationToken);
        var pending = await _itemRepository.GetPendingItemsAsync(cancellationToken);
        return waiting.Concat(pending).Select(MapToQueueItem).ToList();
    }

    public async Task EnqueueOrderAsync(Guid kitchenOrderId, CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(kitchenOrderId, cancellationToken)
            ?? throw new NotFoundException("KitchenOrder", kitchenOrderId);

        order.Enqueue();
        await _repository.UpdateAsync(order, cancellationToken);

        await ScheduleAsync(cancellationToken);
    }

    public async Task FinishItemAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        var item = await _itemRepository.GetItemByIdAsync(itemId, cancellationToken)
            ?? throw new NotFoundException("KitchenOrderItem", itemId);

        if (item.IsFinished)
            return;

        item.Finish();
        await _itemRepository.UpdateItemAsync(item, cancellationToken);

        var order = await _repository.GetByIdWithItemsAsync(item.KitchenOrderId, cancellationToken);
        if (order is not null && order.Items.All(i => i.IsFinished))
        {
            order.MarkReady();
            await _repository.UpdateAsync(order, cancellationToken);
            await _orderServiceClient.NotifyOrderReadyAsync(order.OrderId, cancellationToken);
        }

        await ScheduleAsync(cancellationToken);
    }

    public async Task ScheduleAsync(CancellationToken cancellationToken = default)
    {
        await ScheduleGate.WaitAsync(cancellationToken);
        try
        {
            await TryScheduleAsync(cancellationToken);
        }
        finally
        {
            ScheduleGate.Release();
        }

        await NotifyQueueChangedAsync(cancellationToken);
    }

    private async Task TryScheduleAsync(CancellationToken cancellationToken)
    {
        var maxDishes = await _orchestratorRepository.GetMaxConcurrentDishesAsync(cancellationToken);
        var activeOrders = await _repository.GetActiveOrdersAsync(cancellationToken);
        var usedSlots = activeOrders.Sum(order => order.UsedSlots);

        while (usedSlots < maxDishes)
        {
            var availableSlots = maxDishes - usedSlots;
            var order = await _repository.GetNextWaitingOrderAsync(cancellationToken);

            if (order is null)
                break;

            if (order.Items.Count > availableSlots && usedSlots > 0)
                break;

            _schedulingPolicy.Schedule(order);
            order.StartPreparing();
            await _repository.UpdateAsync(order, cancellationToken);

            usedSlots += order.Items.Count;
        }
    }

    private async Task NotifyQueueChangedAsync(CancellationToken cancellationToken)
    {
        try
        {
            var cooking = await _itemRepository.GetItemsReadyToCookAsync(cancellationToken);
            var waiting = (await _itemRepository.GetItemsToWaitingAsync(cancellationToken))
                .Concat(await _itemRepository.GetPendingItemsAsync(cancellationToken))
                .ToList();

            var snapshot = new KitchenQueueSnapshotDto
            {
                Cooking = cooking.Select(MapToQueueItem).ToList(),
                Waiting = waiting.Select(MapToQueueItem).ToList()
            };

            await _kitchenNotifier.NotifyQueueChangedAsync(snapshot, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo notificar en tiempo real el cambio de la cola de cocina.");
        }
    }

    private static KitchenQueueItemResponse MapToQueueItem(KitchenOrderItem item) => new()
    {
        ItemId = item.Id,
        ProductName = item.ProductName,
        Quantity = item.Quantity,
        EstimatedTime = item.EstimatedTime,
        StartTime = item.StartTime,
        Notes = item.Notes ?? string.Empty
    };
}
