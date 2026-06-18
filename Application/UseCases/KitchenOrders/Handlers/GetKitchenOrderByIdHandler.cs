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
    public class GetKitchenOrderByIdHandler : IGetKitchenOrderByIdHandler
    {
        private readonly IKitchenOrderRepository _repository;

        public GetKitchenOrderByIdHandler(IKitchenOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<KitchenOrderDto> GetKitchenOrderById(GetKitchenOrderByIdQuery query)
        {
            var order = await _repository.GetOrderWithItemsAsync(query.Id);

            if (order == null)
            {
                throw new NotFoundException("KitchenOrder", query.Id);
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
