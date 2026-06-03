using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain.Entities
{
    public class KitchenOrderItem
    {
        public Guid Id { get; set; }
        public Guid KitchenOrderId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int EstimatedTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public ItemStatus Status { get; set; }
        public int PriorityScore { get; set; }
        public int Position { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool IsRushed { get; set; }

        public virtual KitchenOrder Order { get; set; } = null!;
    }
}
