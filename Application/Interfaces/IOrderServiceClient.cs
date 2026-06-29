namespace Application.Interfaces;

public interface IOrderServiceClient
{
    Task NotifyOrderItemReadyAsync(Guid orderId, Guid orderItemId, CancellationToken cancellationToken = default);
    Task NotifyOrderReadyAsync(Guid orderId, bool wasDelayed, CancellationToken cancellationToken = default);
}
