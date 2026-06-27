using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.KitchenOrders.Comands
{
    public class CreateKitchenOrderCommand
    {
        public Guid OrderId { get; set; }
        public Guid TableId { get; set; }
        public int TableNumber { get; set; }
        public Guid WaiterId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public List<CreateKitchenOrderItemDto> Items { get; set; } = new();
    }

    public class CreateKitchenOrderItemDto
    {
        public Guid OrderItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int DurationMinutes { get; set; } // en minutos
        public int Quantity { get; set; } // se agrega quantity que faltaba 
        public decimal FactorMultiplierTime { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
