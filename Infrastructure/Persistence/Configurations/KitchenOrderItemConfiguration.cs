using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class KitchenOrderItemConfiguration : IEntityTypeConfiguration<KitchenOrderItem>
{
    public void Configure(EntityTypeBuilder<KitchenOrderItem> builder)
    {
        builder.ToTable("KitchenOrderItems");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.OrderItemId);

        builder.Property(x => x.OrderItemId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.FactorMultiplierTime)
            .HasPrecision(5, 2);

        builder.Property(x => x.MaxQuantityTimeMultiplier)
            .HasPrecision(5, 2);

        builder.Property(x => x.Version)
            .IsRowVersion();
    }
}
