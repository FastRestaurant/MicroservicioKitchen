using API.Hubs;
using Application.DTOs;
using Application.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace API.Realtime;

public sealed class SignalRKitchenNotifier : IKitchenNotifier
{
    private const string QueueChangedMethod = "QueueChanged";

    private readonly IHubContext<KitchenHub> _hubContext;

    public SignalRKitchenNotifier(IHubContext<KitchenHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyQueueChangedAsync(KitchenQueueSnapshotDto snapshot, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.Groups(KitchenHubGroups.Kitchen, KitchenHubGroups.Admin)
            .SendAsync(QueueChangedMethod, snapshot, cancellationToken);
}
