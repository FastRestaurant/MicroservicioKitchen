using Application.DTOs;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Handlers
{
    public class GetKitchenQueueHandler
    {
        private readonly IKitchenOrderRepository _repository;

        public GetKitchenQueueHandler(IKitchenOrderRepository repository)
        {
            _repository = repository;
        }

        // Método normal que vas a llamar desde tu Controller
        public async Task<List<KitchenOrderDto>> HandleAsync()
        {
            var activeOrders = await _repository.GetActiveOrdersAsync();

            var response = activeOrders.Select(order => new KitchenOrderDto
            {
                Id = order.Id,
                TableNumber = order.TableNumber,
                WaiterName = order.WaiterName,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                TotalItems = order.TotalItems,
                CompletedItems = order.CompletedItems,

                Items = order.Items
                    .OrderByDescending(i => i.PriorityScore)
                    .Select(item => new KitchenOrderItemDto
                    {
                        Id = item.Id,
                        ProductName = item.ProductName,
                        Category = item.Category,
                        EstimatedTime = item.EstimatedTime,
                        StartTime = item.StartTime,
                        FinishTime = item.FinishTime,
                        Status = item.Status.ToString(),
                        PriorityScore = item.PriorityScore
                    }).ToList()
            }).ToList();

            return response;
        }
    }
}
