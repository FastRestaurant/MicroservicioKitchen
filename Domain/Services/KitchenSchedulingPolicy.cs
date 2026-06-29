using Domain.Entities;
using Domain.Enums;

namespace Domain.Services;

public sealed class KitchenSchedulingPolicy
{
    public void Schedule(KitchenOrder order)
    {
        var liveItems = order.Items
            .Where(item => item.Status is ItemStatus.Pending or ItemStatus.Preparing)
            .ToArray();

        if (liveItems.Length == 0)
            return;

        var longestEstimated = liveItems.Max(item => item.ComputeEstimatedTime());
        var targetFinish = DateTime.UtcNow.AddMinutes(longestEstimated);

        foreach (var item in liveItems)
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
