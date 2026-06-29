using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public sealed class KitchenOrder
{
    private readonly List<KitchenOrderItem> _items = new();

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid TableId { get; private set; }
    public int TableNumber { get; private set; }
    public Guid WaiterId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime? ActualFinishTime { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }
    public byte[] Version { get; private set; } = Array.Empty<byte>();

    public IReadOnlyCollection<KitchenOrderItem> Items => _items;

    private KitchenOrder() { }

    public static KitchenOrder Create(Guid orderId, Guid tableId, int tableNumber, Guid waiterId)
    {
        return new KitchenOrder
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            TableId = tableId,
            TableNumber = tableNumber,
            WaiterId = waiterId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(KitchenOrderItem item)
    {
        item.AssignToOrder(Id);
        _items.Add(item);
    }

    public void Enqueue()
    {
        if (Status != OrderStatus.Preparing)
        {
            Status = OrderStatus.Pending;
            ActualFinishTime = null;
        }
        Touch();
    }

    public void StartPreparing()
    {
        Status = OrderStatus.Preparing;
        Touch();
    }

    public void MarkReady()
    {
        if (!_items.All(i => i.IsFinished))
            throw new DomainException("La orden no puede marcarse como lista hasta que todos sus items esten finalizados.");

        Status = OrderStatus.Ready;
        ActualFinishTime = DateTime.UtcNow;
        Touch();
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
            throw new ConflictException("La orden ya esta cancelada.");

        var now = DateTime.UtcNow;
        if (_items.Any(i => !i.CanCancel(now)))
            throw new ConflictException("No se puede cancelar la orden porque ya hay platos en preparación o finalizados.");

        Status = OrderStatus.Cancelled;
        Touch();

        foreach (var item in _items)
            item.Cancel();
    }

    public int UsedSlots => _items.Count(i => i.IsPreparing);
    public int PendingSlots => _items.Count(i => i.Status == ItemStatus.Pending);
    public int ActiveSlots(DateTime now) => _items.Count(i => i.Status == ItemStatus.Preparing && i.StartTime.HasValue && i.StartTime.Value <= now);
    public int UpcomingSlots(DateTime now) => _items.Count(i => i.Status == ItemStatus.Preparing && i.StartTime.HasValue && i.StartTime.Value > now);

    private void Touch() => LastUpdatedAt = DateTime.UtcNow;
}
