using System.Text.Json.Serialization;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public sealed class KitchenOrderItem
{
    public Guid Id { get; private set; }
    public Guid KitchenOrderId { get; private set; }
    public Guid OrderItemId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public int DurationMinutes { get; private set; }
    public decimal FactorMultiplierTime { get; private set; }
    public decimal MaxQuantityTimeMultiplier { get; private set; }
    public int EstimatedTime { get; private set; }
    public DateTime? StartTime { get; private set; }
    public DateTime? FinishTime { get; private set; }
    public ItemStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public byte[] Version { get; private set; } = Array.Empty<byte>();

    [JsonIgnore]
    public KitchenOrder Order { get; private set; } = null!;

    private KitchenOrderItem() { }

    public static KitchenOrderItem Create(
        Guid orderItemId,
        Guid productId,
        string productName,
        int quantity,
        int durationMinutes,
        decimal factorMultiplierTime,
        decimal maxQuantityTimeMultiplier,
        string? notes)
    {
        return new KitchenOrderItem
        {
            Id = Guid.NewGuid(),
            OrderItemId = orderItemId,
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            DurationMinutes = durationMinutes,
            FactorMultiplierTime = factorMultiplierTime,
            MaxQuantityTimeMultiplier = maxQuantityTimeMultiplier,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes,
            Status = ItemStatus.Pending
        };
    }

    public int ComputeEstimatedTime()
    {
        var multiplier = 1 + (Quantity - 1) * FactorMultiplierTime;
        var cappedMultiplier = Math.Min(multiplier, MaxQuantityTimeMultiplier);

        return (int)Math.Round(DurationMinutes * cappedMultiplier, MidpointRounding.AwayFromZero);
    }

    public void Schedule(DateTime targetFinish)
    {
        EstimatedTime = ComputeEstimatedTime();
        StartTime = targetFinish.AddMinutes(-EstimatedTime);
        FinishTime = targetFinish;
        Status = ItemStatus.Preparing;
    }

    public void Finish()
    {
        if (Status == ItemStatus.Finished)
            return;

        Status = ItemStatus.Finished;
        FinishTime = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == ItemStatus.Finished)
            throw new DomainException("No se puede cancelar un item que ya fue finalizado.");

        Status = ItemStatus.Cancelled;
    }

    public bool CanCancel(DateTime now)
        => Status == ItemStatus.Pending ||
           Status == ItemStatus.Cancelled ||
           (Status == ItemStatus.Preparing && StartTime.HasValue && StartTime.Value > now);

    public bool IsPreparing => Status == ItemStatus.Preparing;
    public bool IsFinished => Status == ItemStatus.Finished;

    internal void AssignToOrder(Guid kitchenOrderId) => KitchenOrderId = kitchenOrderId;
}
