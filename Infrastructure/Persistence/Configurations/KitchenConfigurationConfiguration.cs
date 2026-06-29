using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class KitchenConfigurationConfiguration : IEntityTypeConfiguration<KitchenConfiguration>
{
    public void Configure(EntityTypeBuilder<KitchenConfiguration> builder)
    {
        builder.ToTable("KitchenConfigurations");

        builder.Property(x => x.FactorMultiplierTime)
            .HasPrecision(5, 2);

        builder.Property(x => x.MaxQuantityTimeMultiplier)
            .HasPrecision(5, 2);
    }
}
