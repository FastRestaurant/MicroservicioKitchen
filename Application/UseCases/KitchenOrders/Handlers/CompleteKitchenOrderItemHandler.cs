using Application.Interfaces;
using Application.UseCases.KitchenOrders.Comands;
using Domain.Enums;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.KitchenOrders.Handlers
{
    public class CompleteKitchenOrderItemHandler : ICompleteKitchenOrderItemHandler
    {
        private readonly IKitchenOrderRepository _orderRepository;

        public CompleteKitchenOrderItemHandler(IKitchenOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task ExecuteAsync(CompleteKitchenOrderItemCommand command)
        {
            // 1. Obtener la orden completa que contiene este ítem específico
            var order = await _orderRepository.GetOrderByItemIdAsync(command.ItemId);
            if (order == null)
            {
                throw new NotFoundException("KitchenOrderItem", command.ItemId);
            }

            // 2. Localizar el ítem exacto dentro de la colección
            var itemToComplete = order.Items.First(i => i.Id == command.ItemId);

            if (itemToComplete.Status == ItemStatus.Ready)
            {
                throw new ConflictException("El plato ya se encuentra marcado como listo.");
            }

            // 3. Actualizar el estado del ítem individual
            itemToComplete.Status = ItemStatus.Ready;
            itemToComplete.FinishTime = DateTime.UtcNow;

            // 4. REGLA DE ORO: Validar si todos los ítems de la orden asociada están listos
            bool allItemsReady = order.Items.All(i => i.Status == ItemStatus.Ready || i.Status == ItemStatus.Cancelled);

            if (allItemsReady)
            {
                order.Status = OrderStatus.Ready;
                order.ActualFinishTime = DateTime.UtcNow;
                order.CompletedItems = order.TotalItems;
            }
            else
            {
                // Si pasa a estar en preparación la orden general
                order.Status = OrderStatus.Preparing;
                order.CompletedItems = order.Items.Count(i => i.Status == ItemStatus.Ready);
            }

            order.LastUpdatedAt = DateTime.UtcNow;

            // 5. Persistir todos los cambios atómicamente mediante el repositorio
            await _orderRepository.UpdateAsync(order);
        }
    }
}
