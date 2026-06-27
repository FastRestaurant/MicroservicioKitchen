using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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
        public int Quantity { get; set; }
        public int DurationMinutes { get; set; }
        public decimal FactorMultiplierTime { get; set; }
        public int EstimatedTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public ItemStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;

        [JsonIgnore] // cambiar luego por los DTOs y eliminar linea
        public virtual KitchenOrder Order { get; set; } = null!;
    }
}
