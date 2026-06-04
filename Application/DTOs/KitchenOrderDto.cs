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
        public int TableNumber { get; set; }
        public string WaiterName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TotalItems { get; set; }
        public int CompletedItems { get; set; }
        public List<KitchenOrderItemDto> Items { get; set; } = new List<KitchenOrderItemDto>();
    }
}
