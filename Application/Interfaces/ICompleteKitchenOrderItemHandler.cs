using Application.UseCases.KitchenOrders.Comands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICompleteKitchenOrderItemHandler
    {
        Task ExecuteAsync(CompleteKitchenOrderItemCommand command);
    }
}
