using Application.Interfaces;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Handlers
{
    public class StartItemPreparationHandler
    {
        private readonly IKitchenOrderRepository _repository;

        public StartItemPreparationHandler(IKitchenOrderRepository repository)
        {
            _repository = repository;
        }

        // Recibe el Id del plato (KitchenOrderItem)
        public async Task<bool> HandleAsync(Guid orderItemId)
        {
            // 1. Buscamos la orden padre en la BD usando el método nuevo
            var order = await _repository.GetOrderByItemIdAsync(orderItemId);

            if (order == null) return false;

            // 2. Buscamos el ítem específico dentro de esa orden
            var item = order.Items.FirstOrDefault(i => i.Id == orderItemId);

            if (item == null) return false;

            // 3. Regla: Si el estado es Pending, lo cambiamos a Preparing
            if (item.Status == ItemStatus.Pending)
            {
                item.Status = ItemStatus.Preparing;
                item.StartTime = DateTime.UtcNow; // Seteamos el StartTime

                // 4. Regla: Si la orden padre estaba Pending, la pasamos a Preparing
                if (order.Status == OrderStatus.Pending)
                {
                    order.Status = OrderStatus.Preparing;
                }

                // 5. Guardamos los cambios
                await _repository.UpdateAsync(order);
            }

            return true;
        }
    }
}
