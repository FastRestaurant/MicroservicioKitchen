namespace Application.DTOs;

public sealed class KitchenQueueSnapshotDto
{
    public IReadOnlyCollection<KitchenQueueItemResponse> Cooking { get; init; } = Array.Empty<KitchenQueueItemResponse>();
    public IReadOnlyCollection<KitchenQueueItemResponse> Waiting { get; init; } = Array.Empty<KitchenQueueItemResponse>();
}
