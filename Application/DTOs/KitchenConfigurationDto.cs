namespace Application.DTOs;

public sealed class KitchenConfigurationDto
{
    public int MaxConcurrentDishes { get; init; }
    public decimal FactorMultiplierTime { get; init; }
    public decimal MaxQuantityTimeMultiplier { get; init; }
}
