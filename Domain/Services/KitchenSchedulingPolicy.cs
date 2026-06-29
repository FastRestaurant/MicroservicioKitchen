using Domain.Entities;
using Domain.Enums;

namespace Domain.Services;

public sealed class KitchenSchedulingPolicy
{
    public void Schedule(KitchenOrder order)
    {
        var pendingItems = order.Items
            .Where(item => item.Status == ItemStatus.Pending)
            .ToArray();

        if (pendingItems.Length == 0)
            return;

        var longestEstimated = pendingItems.Max(item => item.ComputeEstimatedTime());
        var targetFinish = DateTime.UtcNow.AddMinutes(longestEstimated);

        foreach (var item in pendingItems)
            item.Schedule(targetFinish);
    }

    public void AdvanceUpcoming(KitchenOrder order)
    {
        var now = DateTime.UtcNow;
        var upcomingItems = order.Items
            .Where(item => item.Status == ItemStatus.Preparing && item.StartTime.HasValue && item.StartTime.Value > now)
            .ToArray();

        if (upcomingItems.Length == 0)
            return;

        var longestEstimated = upcomingItems.Max(item => item.ComputeEstimatedTime());
        var targetFinish = now.AddMinutes(longestEstimated);

        foreach (var item in upcomingItems)
            item.Schedule(targetFinish);
    }
}
