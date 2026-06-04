using Application.Interfaces;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Handlers
{
    public class CancelItemHandler
    {
        private readonly IKitchenOrderRepository _repository;

        public CancelItemHandler(IKitchenOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> HandleAsync(Guid orderItemId)
        {
            // 1. Buscamos la orden padre y sus ítems
            var order = await _repository.GetOrderByItemIdAsync(orderItemId);
            if (order == null) return false;

            // 2. Buscamos el ítem específico
            var item = order.Items.FirstOrDefault(i => i.Id == orderItemId);
            if (item == null || item.Status == ItemStatus.Cancelled) return false;

            // Seguridad extra: Si cancelan un plato que ya estaba "Ready", 
            // tenemos que restarle 1 también a CompletedItems para no desfasar la cuenta.
            if (item.Status == ItemStatus.Ready)
            {
                order.CompletedItems--;
            }

            // 3. Marcamos el plato como cancelado
            item.Status = ItemStatus.Cancelled;

            // 4. LA REGLA ESTRICTA: Restamos 1 al TotalItems de la orden padre
            order.TotalItems--;

            // 5. Evaluamos el estado final de la orden
            if (order.TotalItems == 0)
            {
                // Si cancelaron TODOS los platos de la mesa, la orden entera se cancela
                order.Status = OrderStatus.Cancelled;
            }
            else if (order.CompletedItems == order.TotalItems)
            {
                // Si al cancelar este plato, los demás ya estaban listos, la orden queda lista
                order.Status = OrderStatus.Ready;
                order.ActualFinishTime = DateTime.UtcNow;
            }

            // 6. Guardamos los cambios
            await _repository.UpdateAsync(order);

            return true;
        }
    }
}
