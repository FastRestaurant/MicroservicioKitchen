using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.UseCases.KitchenOrders.Comands;

namespace Application.Interfaces
{
    public interface IRecalculateOrderHandler
    {
        Task<KitchenOrderDto> RecalculateOrder(RecalculateOrderCommand command);
    }
}
