using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain.Entities
{
    public class KitchenOrder
    {
        public Guid Id { get; set; }


        public Guid OrderId { get; set; }
        public Guid TableId { get; set; }
        public int TableNumber { get; set; }
        public Guid WaiterId { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime? ActualFinishTime { get; set; }
        public DateTime LastUpdatedAt { get; set; }

        public virtual ICollection<KitchenOrderItem> Items { get; set; } = new List<KitchenOrderItem>();
    }

}
