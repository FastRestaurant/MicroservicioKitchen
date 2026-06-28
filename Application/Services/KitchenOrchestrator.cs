using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public sealed class KitchenOrchestrator : IKitchenOrchestrator
{
    private static readonly SemaphoreSlim ScheduleGate = new(1, 1);

    private readonly IKitchenOrderRepository _repository;
    private readonly IKitchenOrderItemRepository _itemRepository;
    private readonly IKitchenOrchestratorRepository _orchestratorRepository;
    private readonly IOrderServiceClient _orderServiceClient;
    private readonly ILogger<KitchenOrchestrator> _logger;

    public KitchenOrchestrator(
        IKitchenOrderRepository repository,
        IKitchenOrchestratorRepository orchestratorRepository,
        IKitchenOrderItemRepository itemRepository,
        IOrderServiceClient orderServiceClient,
        ILogger<KitchenOrchestrator> logger)
    {
        _repository = repository;
        _orchestratorRepository = orchestratorRepository;
        _itemRepository = itemRepository;
        _orderServiceClient = orderServiceClient;
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

        order.Status = OrderStatus.Pending;
        order.LastUpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(order, cancellationToken);

        await ScheduleAsync(cancellationToken);
    }

    public async Task FinishItemAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        var item = await _itemRepository.GetItemByIdAsync(itemId, cancellationToken)
            ?? throw new NotFoundException("KitchenOrderItem", itemId);

        if (item.Status == ItemStatus.Finished)
            return;

        item.Status = ItemStatus.Finished;
        item.FinishTime = DateTime.UtcNow;
        await _itemRepository.UpdateItemAsync(item, cancellationToken);

        if (await CheckAndCompleteOrderAsync(item.KitchenOrderId, cancellationToken))
        {
            var order = await _repository.GetByIdAsync(item.KitchenOrderId, cancellationToken);
            if (order is not null)
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
    }

    private async Task TryScheduleAsync(CancellationToken cancellationToken)
    {
        var maxDishes = await _orchestratorRepository.GetMaxConcurrentDishesAsync(cancellationToken);
        var activeOrders = await _repository.GetActiveOrdersAsync(cancellationToken);

        var usedSlots = activeOrders
            .SelectMany(o => o.Items)
            .Count(i => i.Status == ItemStatus.Preparing);

        while (usedSlots < maxDishes)
        {
            var availableSlots = maxDishes - usedSlots;
            var order = await _repository.GetNextWaitingOrderAsync(cancellationToken);

            if (order is null)
                break;

            if (order.Items.Count > availableSlots && usedSlots > 0)
                break;

            CalculateSyncTimes(order);
            order.Status = OrderStatus.Preparing;
            order.LastUpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(order, cancellationToken);

            usedSlots += order.Items.Count;
        }
    }

    private static void CalculateSyncTimes(KitchenOrder order)
    {
        if (order.Items is null || order.Items.Count == 0)
            return;

        foreach (var item in order.Items)
        {
            item.EstimatedTime = (int)Math.Round(
                item.DurationMinutes * (1 + (item.Quantity - 1) * item.FactorMultiplierTime),
                MidpointRounding.AwayFromZero);
        }

        var maxTime = order.Items.Max(i => i.EstimatedTime);
        var targetFinish = DateTime.UtcNow.AddMinutes(maxTime);

        foreach (var item in order.Items)
        {
            item.StartTime = targetFinish.AddMinutes(-item.EstimatedTime);
            item.FinishTime = targetFinish;
            item.Status = ItemStatus.Preparing;
        }
    }

    private async Task<bool> CheckAndCompleteOrderAsync(Guid kitchenOrderId, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdWithItemsAsync(kitchenOrderId, cancellationToken);
        if (order is null)
            return false;

        if (!order.Items.All(i => i.Status == ItemStatus.Finished))
            return false;

        order.Status = OrderStatus.Ready;
        order.ActualFinishTime = DateTime.UtcNow;
        order.LastUpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(order, cancellationToken);

        return true;
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
