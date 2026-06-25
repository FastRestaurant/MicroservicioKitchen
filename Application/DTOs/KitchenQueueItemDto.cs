using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class KitchenQueueItemDto
    {
        public Guid ItemId { get; set; }
        public Guid KitchenOrderId { get; set; }
        public int TableNumber { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int EstimatedTime { get; set; }
        public DateTime? StartTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public int PriorityScore { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool IsRushed { get; set; }
    }
}
