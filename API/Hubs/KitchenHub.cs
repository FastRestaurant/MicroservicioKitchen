using System.Security.Claims;
using API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace API.Hubs;

[Authorize]
public sealed class KitchenHub : Hub
{
    private readonly ILogger<KitchenHub> _logger;

    public KitchenHub(ILogger<KitchenHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var groups = ResolveGroups();

        if (groups.Count == 0)
            _logger.LogWarning(
                "La conexion {ConnectionId} del usuario {UserIdentifier} no resolvio ningun grupo de cocina.",
                Context.ConnectionId,
                Context.UserIdentifier);

        foreach (var group in groups)
            await Groups.AddToGroupAsync(Context.ConnectionId, group);

        await base.OnConnectedAsync();
    }

    private List<string> ResolveGroups()
    {
        var roles = Context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToHashSet() ?? new HashSet<string>();
        var groups = new List<string>();

        if (roles.Contains(ApplicationRoles.Admin))
        {
            groups.Add(KitchenHubGroups.Admin);
            groups.Add(KitchenHubGroups.Kitchen);
            return groups;
        }

        if (roles.Contains(ApplicationRoles.Kitchen))
            groups.Add(KitchenHubGroups.Kitchen);

        return groups;
    }
}
