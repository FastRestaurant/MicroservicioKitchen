using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.KitchenOrders.Queries
{
    public class GetKitchenOrderByOrderIdQuery
    {
        public Guid OrderId { get; set; }
    }
}
