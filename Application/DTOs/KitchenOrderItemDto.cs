using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class KitchenOrderItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; } // ← FALTABA
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int EstimatedTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public int PriorityScore { get; set; }
        public int Position { get; set; } // ← FALTABA
        public string Notes { get; set; } = string.Empty; // ← FALTABA
        public bool IsRushed { get; set; } // ← FALTABA
    }
}