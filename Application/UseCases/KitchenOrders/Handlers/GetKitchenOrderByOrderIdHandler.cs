using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.KitchenOrders.Queries;
using Domain.Exceptions;

namespace Application.UseCases.KitchenOrders.Handlers
{
    public class GetKitchenOrderByOrderIdHandler : IGetKitchenOrderByOrderIdHandler
    {
        private readonly IKitchenOrderRepository _repository;

        public GetKitchenOrderByOrderIdHandler(IKitchenOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<KitchenOrderDto> GetKitchenOrderByOrderId(GetKitchenOrderByOrderIdQuery query)
        {
            var order = await _repository.GetByOrderIdAsync(query.OrderId);

            if (order == null)
            {
                throw new NotFoundException("KitchenOrder", query.OrderId);
            }

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
