using Application.DTOs;

namespace Application.Realtime;

public interface IKitchenNotifier
{
    Task NotifyQueueChangedAsync(KitchenQueueSnapshotDto snapshot, CancellationToken cancellationToken = default);
}
