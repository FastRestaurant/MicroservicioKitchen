using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class KitchenQueueItemResponse
    {
        public Guid ItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int EstimatedTime { get; set; }
        public DateTime? StartTime { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
