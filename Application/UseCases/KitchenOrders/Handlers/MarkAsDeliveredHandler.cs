using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.KitchenOrders.Comands;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.UseCases.KitchenOrders.Handlers
{
    public class MarkAsDeliveredHandler : IMarkAsDeliveredHandler
    {
        private readonly IKitchenOrderRepository _repository;

        public MarkAsDeliveredHandler(IKitchenOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<KitchenOrderDto> MarkAsDelivered(MarkAsDeliveredCommand command)
        {
            var order = await _repository.GetOrderWithItemsAsync(command.Id);

            if (order == null)
            {
                throw new NotFoundException("KitchenOrder", command.Id);
            }

            // Validación: solo se puede entregar si está Ready
            if (order.Status != OrderStatus.Ready)
            {
                throw new ConflictException(
                    $"La orden no puede ser entregada. Estado actual: {order.Status}. " +
                    $"Debe estar en estado 'Ready' para ser entregada.");
            }

            // Marcar como entregada
            order.Status = OrderStatus.Delivered;
            order.ActualFinishTime = DateTime.UtcNow;
            order.LastUpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(order);

            return MapToDto(order);
        }

        private KitchenOrderDto MapToDto(Domain.Entities.KitchenOrder order)
        {
            return new KitchenOrderDto
            {
                Id = order.Id,
                OrderId = order.OrderId,
                TableNumber = order.TableNumber,
                WaiterName = order.WaiterName,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                EstimatedFinishTime = order.EstimatedFinishTime,
                ActualFinishTime = order.ActualFinishTime,
                TotalItems = order.TotalItems,
                CompletedItems = order.CompletedItems,
                LastUpdatedAt = order.LastUpdatedAt,
                Items = order.Items.Select(item => new KitchenOrderItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Category = item.Category,
                    EstimatedTime = item.EstimatedTime,
                    StartTime = item.StartTime,
                    FinishTime = item.FinishTime,
                    Status = item.Status.ToString(),
                    PriorityScore = item.PriorityScore,
                    Position = item.Position,
                    Notes = item.Notes,
                    IsRushed = item.IsRushed
                }).ToList()
            };
        }
    }
}
