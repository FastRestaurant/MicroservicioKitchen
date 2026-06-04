using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.KitchenOrders.Comands;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.UseCases.KitchenOrders.Handlers
{
    public class RecalculateOrderHandler : IRecalculateOrderHandler
    {
        private readonly IKitchenOrderRepository _repository;

        public RecalculateOrderHandler(IKitchenOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<KitchenOrderDto> RecalculateOrder(RecalculateOrderCommand command)
        {
            //PASO 1: Buscar la orden
            var order = await _repository.GetOrderWithItemsAsync(command.OrderId);
            if (order == null)
            {
                throw new NotFoundException("KitchenOrder", command.OrderId);
            }

            // PASO 2: Buscar el item afectado
            var affectedItem = order.Items.FirstOrDefault(i => i.Id == command.ItemId);
            if (affectedItem == null)
            {
                throw new NotFoundException("KitchenOrderItem", command.ItemId);
            }

            // PASO 3: Validar que el item se pueda recalcular 
            if (affectedItem.Status == ItemStatus.Ready)
            {
                throw new ConflictException(
                    $"No se puede recalcular el item '{affectedItem.ProductName}' porque ya está listo para servir.");
            }

            if (affectedItem.Status == ItemStatus.Cancelled || affectedItem.Status == ItemStatus.Ruined)
            {
                throw new ConflictException(
                    $"El item '{affectedItem.ProductName}' ya fue marcado como {affectedItem.Status}.");
            }

            // PASO 4: Marcar el item según el motivo 
            if (command.Reason == RecalculationReason.ItemRuined)
            {
                affectedItem.Status = ItemStatus.Ruined;
                affectedItem.FinishTime = DateTime.UtcNow;
            }
            else if (command.Reason == RecalculationReason.ItemCancelled)
            {
                affectedItem.Status = ItemStatus.Cancelled;
                affectedItem.FinishTime = DateTime.UtcNow;

                // Si se cancela, restar 1 al TotalItems (para que CompleteItem funcione bien)
                order.TotalItems--;
            }

            //PASO 5: Obtener items que aún están "activos" 
            // Activos = Pending o Preparing (no Ready, Cancelled ni Ruined)
            var activeItems = order.Items
                .Where(i => i.Status == ItemStatus.Pending || i.Status == ItemStatus.Preparing)
                .ToList();

            // Si no hay items activos, no hay nada que recalcular
            if (!activeItems.Any())
            {
                order.LastUpdatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(order);
                return MapToResponseDto(order);
            }

            //PASO 6: RECÁLCULO DE TIEMPOS 
            RecalculateTimes(order, activeItems);

            //PASO 7: Actualizar la orden 
            order.LastUpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(order);

            return MapToResponseDto(order);
        }

       
        /// Motor de Recálculo: recalcula los tiempos de los items pendientes
        /// para que sigan sincronizados con los items en preparación.
       
        private void RecalculateTimes(KitchenOrder order, List<KitchenOrderItem> activeItems)
        {
            // Separar items en preparación de los pendientes
            var ruinedItems = order.Items.Where(i => i.Status == ItemStatus.Ruined).ToList();
            var preparingItems = activeItems.Where(i => i.Status == ItemStatus.Preparing).ToList();
            var pendingItems = activeItems.Where(i => i.Status == ItemStatus.Pending).ToList();

            DateTime newTargetFinishTime;

            var allItemsToConsider = preparingItems.Concat(pendingItems).Concat(ruinedItems).ToList();

            if (preparingItems.Any() || ruinedItems.Any())
            {
                // Calcular el máximo finish time entre:
                // 1. Items en preparación (ya empezaron)
                // 2. Items arruinados (van a reheacerse desde ahora)
                // 3. Items pendientes (si empezaran ahora)

                var preparingFinishTimes = preparingItems
                    .Where(i => i.StartTime.HasValue)
                    .Select(i => i.StartTime!.Value.AddMinutes(i.EstimatedTime));

                var ruinedFinishTimes = ruinedItems
                    .Select(i => DateTime.UtcNow.AddMinutes(i.EstimatedTime));

                var pendingFinishTimes = pendingItems.Any()
                    ? new[] { DateTime.UtcNow.AddMinutes(pendingItems.Max(i => i.EstimatedTime)) }
                    : Enumerable.Empty<DateTime>();

                var allFinishTimes = preparingFinishTimes
                    .Concat(ruinedFinishTimes)
                    .Concat(pendingFinishTimes);

                newTargetFinishTime = allFinishTimes.Any()
                    ? allFinishTimes.Max()
                    : DateTime.UtcNow;
            }
            else
            {
                // No hay items en preparación ni arruinados: recalcular desde ahora
                var maxTime = pendingItems.Max(i => i.EstimatedTime);
                newTargetFinishTime = DateTime.UtcNow.AddMinutes(maxTime);
            }

            // Recalcular StartTime de los items pendientes
            foreach (var item in pendingItems)
            {
                item.StartTime = newTargetFinishTime.AddMinutes(-item.EstimatedTime);

                // Si el StartTime calculado es en el pasado, empezar YA
                if (item.StartTime < DateTime.UtcNow)
                {
                    item.StartTime = DateTime.UtcNow;
                }
            }

            foreach (var item in ruinedItems)
            {
                item.StartTime = newTargetFinishTime.AddMinutes(-item.EstimatedTime);

                if (item.StartTime < DateTime.UtcNow)
                {
                    item.StartTime = DateTime.UtcNow;
                }

                // Resetear el finishTime (va a ser recalculado cuando se complete)
                item.FinishTime = null;
            }

            // Actualizar el EstimatedFinishTime de la orden
            order.EstimatedFinishTime = newTargetFinishTime;
        }

        private KitchenOrderDto MapToResponseDto(KitchenOrder order)
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

