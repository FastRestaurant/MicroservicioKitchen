using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.KitchenOrders.Comands
{
    public class RecalculateOrderCommand
    {
        public Guid OrderId { get; set; }
        public Guid ItemId { get; set; }
        public RecalculationReason Reason { get; set; }
        public string? ReasonDetail { get; set; } 
    }

    public enum RecalculationReason
    {
        ItemRuined = 0,      
        ItemCancelled = 1    
    }
}
