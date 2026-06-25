using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.KitchenOrders.Comands
{
    public class CompleteKitchenOrderItemCommand
    {
        public Guid ItemId { get; set; }
    }
}
