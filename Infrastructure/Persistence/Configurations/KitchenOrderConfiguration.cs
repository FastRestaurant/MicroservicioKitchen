using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class KitchenOrderConfiguration : IEntityTypeConfiguration<KitchenOrder>
    {
        public void Configure(EntityTypeBuilder<KitchenOrder> builder)
        {
            builder.ToTable("KitchenOrders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrderId)
                .IsRequired();

            builder.Property(x => x.TableId)
                .IsRequired();

            builder.Property(x => x.TableNumber)
                .IsRequired();

            builder.Property(x => x.WaiterId)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();


            builder.HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.KitchenOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices útiles para cocina
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.CreatedAt);

        }
    }
}
