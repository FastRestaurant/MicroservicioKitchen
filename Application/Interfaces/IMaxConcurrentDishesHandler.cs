using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.UseCases.KitchenOrders.Comands;

namespace Application.Interfaces
{
    public interface IMaxConcurrentDishesHandler
    {
        Task ExecuteAsync(UpdateMaxConcurrentDishesCommand command);
    }
}
