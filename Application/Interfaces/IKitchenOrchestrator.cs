using Application.DTOs;

namespace Application.Interfaces;

public interface IKitchenOrchestrator
{
    Task EnqueueOrderAsync(Guid kitchenOrderId, CancellationToken cancellationToken = default);
    Task EnqueueOrderWithoutNotificationAsync(Guid kitchenOrderId, CancellationToken cancellationToken = default);
    Task<List<KitchenQueueItemResponse>> GetItemsFromQueueAsync(CancellationToken cancellationToken = default);
    Task<List<KitchenQueueItemResponse>> GetWaitingItemsAsync(CancellationToken cancellationToken = default);
    Task FinishItemAsync(Guid itemId, CancellationToken cancellationToken = default);
    Task NotifyQueueChangedAsync(CancellationToken cancellationToken = default);

    Task ScheduleAsync(CancellationToken cancellationToken = default);
}
