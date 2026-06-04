using Application.Interfaces;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Handlers
{
    public class CompleteItemHandler
    {
        private readonly IKitchenOrderRepository _repository;

        public CompleteItemHandler(IKitchenOrderRepository repository)
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

            if (item == null) return false;

            // Validación de seguridad: Evitar procesar un ítem que ya está listo
            if (item.Status == ItemStatus.Ready) return true;

            // 3. Marcamos el plato como terminado
            item.Status = ItemStatus.Ready;
            item.FinishTime = DateTime.UtcNow;

            // 4. LA REGLA DE ORO: Incrementamos los platos completados en la orden
            order.CompletedItems++;

            // 5. Condición de cierre: ¿Están todos los platos de esta mesa listos?
            if (order.CompletedItems == order.TotalItems)
            {
                order.Status = OrderStatus.Ready;
                // Opcional: También podríamos setear el ActualFinishTime de la orden acá
                order.ActualFinishTime = DateTime.UtcNow;
            }

            // 6. Guardamos todos los cambios en la base de datos
            await _repository.UpdateAsync(order);

            return true;
        }
    }
}