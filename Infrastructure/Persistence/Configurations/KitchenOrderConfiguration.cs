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
            builder.HasKey(k => k.Id);

            builder.Property(k => k.OrderId).IsRequired();
            builder.Property(k => k.TableNumber).IsRequired();
            builder.Property(k => k.WaiterName).HasMaxLength(100);
            builder.Property(k => k.Status).HasConversion<string>().IsRequired();
            builder.Property(k => k.CreatedAt).IsRequired();

            builder.HasMany(k => k.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.KitchenOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
