using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class KitchenOrderDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; } // ← FALTABA (referencia al Order Service)
        public int TableNumber { get; set; }
        public string WaiterName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? EstimatedFinishTime { get; set; } // ← FALTABA
        public DateTime? ActualFinishTime { get; set; } // ← FALTABA
        public int TotalItems { get; set; }
        public int CompletedItems { get; set; }
        public DateTime LastUpdatedAt { get; set; } // ← FALTABA
        public List<KitchenOrderItemDto> Items { get; set; } = new();
    }
}
