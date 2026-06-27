using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Enums;

namespace Application.UseCases.KitchenOrders.Handlers
{
    public class CancelKitchenOrderHandler : ICancelKitchenOrderHandler
    {
        private readonly IKitchenOrderRepository _repository;

        public CancelKitchenOrderHandler(IKitchenOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(Guid kitchenOrderId)
        {
            var order = await _repository.GetByIdAsync(kitchenOrderId);

            if (order == null)
                throw new KeyNotFoundException("Kitchen order not found.");

            if (order.Status == OrderStatus.Ready)
                throw new InvalidOperationException("A completed order cannot be cancelled.");

            if (order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("The order is already cancelled.");

            order.Status = OrderStatus.Cancelled;
            order.LastUpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(order);
        }
    }
}
