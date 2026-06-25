using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.UseCases.KitchenOrders.Comands;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.UseCases.KitchenOrders.Handlers
{
    public class CreateKitchenOrderHandler : ICreateKitchenOrderHandler
    {
        private readonly IKitchenOrderRepository _repository;
        private readonly IKitchenOrchestrator _orchestrator;
        public CreateKitchenOrderHandler(IKitchenOrderRepository repository, IKitchenOrchestrator orchestrator)
        {
            _repository = repository;
            _orchestrator = orchestrator;
        }

        public async Task<KitchenOrder> CreateKitchenOrder(CreateKitchenOrderCommand command)
        {
            // 1. Validar que haya items
            if (command.Items == null || !command.Items.Any())
            {
                throw new ValidationExceptions(new Dictionary<string, string[]>
            {
                { "items", new[] { "La orden debe contener al menos un item." } }
            });
            }

            // 2. Validar número de mesa
            if (command.TableNumber <= 0)
            {
                throw new ValidationExceptions(new Dictionary<string, string[]>
            {
                { "tableNumber", new[] { "El número de mesa debe ser mayor a 0." } }
            });
            }


            // 4. Validar cada item
            var validationErrors = new Dictionary<string, string[]>();
            for (int i = 0; i < command.Items.Count; i++)
            {
                var item = command.Items[i];
                var itemErrors = new List<string>();

                if (item.ProductId == Guid.Empty)
                    itemErrors.Add("El ProductId es obligatorio.");

                if (string.IsNullOrWhiteSpace(item.ProductName))
                    itemErrors.Add("El nombre del producto es obligatorio.");

                if (item.DurationMinutes <= 0)
                    itemErrors.Add("El tiempo estimado debe ser mayor a 0 minutos.");

                if (itemErrors.Any())
                {
                    validationErrors[$"items[{i}]"] = itemErrors.ToArray();
                }
            }

            if (validationErrors.Any())
            {
                throw new ValidationExceptions(validationErrors);
            }
            // 1. Crear la orden principal
            var order = new KitchenOrder
            {
                Id = Guid.NewGuid(),
                OrderId = command.OrderId,
                TableNumber = command.TableNumber,
                WaiterId = command.WaiterId,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                TotalItems = command.Items.Count,
                CompletedItems = 0,
                LastUpdatedAt = DateTime.UtcNow,
                Items = new List<KitchenOrderItem>()
            };

            // 2. Crear los items de la orden
            foreach (var itemDto in command.Items)
            {
                var item = new KitchenOrderItem
                {
                    Id = Guid.NewGuid(),
                    KitchenOrderId = order.Id,
                    ProductId = itemDto.ProductId,
                    ProductName = itemDto.ProductName,
                    EstimatedTime = itemDto.DurationMinutes,
                    StartTime = null,
                    FinishTime = null,
                    Status = ItemStatus.Pending,
                    PriorityScore = 0, // Se calcula en el algoritmo
                    Position = 0,
                    Notes = itemDto.Notes,
                    IsRushed = false
                };

                order.Items.Add(item);
            }
            // 3. Guardar en la base de datos
            var savedOrder = await _repository.CreateAsync(order);

            // 4. Llamar al Orchestrator para calcular los tiempos
            await _orchestrator.EnqueueOrderAsync(savedOrder.Id);

            // 5. Recargar la orden con los tiempos actualizados
            return await _repository.GetByIdAsync(savedOrder.Id);
        }
    }
}
