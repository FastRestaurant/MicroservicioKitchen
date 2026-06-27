using Application.DTOs;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain.Entities;
using Domain.Enums;


namespace Application.service
{
    public class KitchenOrchestrator : IKitchenOrchestrator
    {
        private readonly IOrderServiceClient _orderServiceClient;
        private readonly IKitchenOrderRepository _repository;
        private readonly IKitchenOrderItemRepository _itemRepository;
        private readonly IKitchenOrchestratorRepository _orchestratorRepository;
        public KitchenOrchestrator(IKitchenOrderRepository repository,
                            IKitchenOrchestratorRepository orchestratorRepository,
                            IKitchenOrderItemRepository itemRepository,
                            IOrderServiceClient orderServiceClient )

        {
            _repository = repository;
            _orchestratorRepository = orchestratorRepository;
            _itemRepository = itemRepository;
            _orderServiceClient = orderServiceClient;
        }

        
        public async Task<List<KitchenQueueItemResponse>> GetItemsFromQueueAsync()
        {
            var listToCooking = await _itemRepository.GetItemsReadyToCookAsync();

            return listToCooking.Select(item => new KitchenQueueItemResponse
            {
                ItemId = item.Id,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                EstimatedTime = item.EstimatedTime,
                StartTime = item.StartTime,
                Notes = item.Notes
            }).ToList();
        }

        public async Task<List<KitchenQueueItemResponse>> GetWaitingItemsAsync()
        {
            var listToWaiting = await _itemRepository.GetItemsReadyToWaitingAsync();

            return listToWaiting.Select(item => new KitchenQueueItemResponse
            {
                ItemId = item.Id,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                EstimatedTime = item.EstimatedTime,
                StartTime = item.StartTime,
                Notes = item.Notes
            }).ToList();
        }


        // recive un una orden nueva y se fija si la puede encolar 
        public async Task EnqueueOrderAsync(Guid kitchenOrderId)
        {
            var order = await _repository.GetByIdAsync(kitchenOrderId);

            if (order == null)
                throw new Exception("Order not found");


            order.Status = Domain.Enums.OrderStatus.Pending;

            await _repository.UpdateAsync(order);

            await TryScheduleAsync(); // intentar meter una orden en cocina
        }


        // marca un plato como finalizado
        public async Task FinishItemAsync(Guid itemId)
        {
            var item = await _itemRepository.GetItemByIdAsync(itemId);

            item.Status = Domain.Enums.ItemStatus.Finished;
            item.FinishTime = DateTime.UtcNow;

            await _itemRepository.UpdateItemAsync(item);

            var completedOrder = await CheckAndCompleteOrderAsync(item.KitchenOrderId);

            if (completedOrder != false)
            {
           // aca habria que llamar a la funcion que notifica a orden que el pedido con el id tanto esta listo 
               
                await _orderServiceClient.NotifyOrderReady(item.KitchenOrderId);
            }

            await TryScheduleAsync(); // se libera espacio y si puede mete otra orden en la cola
        }


        // motor de encolacion en cocina 
        private async Task TryScheduleAsync()
        {
            int maxDishes = await _orchestratorRepository.GetMaxConcurrentDishesAsync();

            // trae las ordenes que estan en la cola 
            var activeOrders = await _repository.GetActiveOrdersAsync();

            // cuenta los items activos de cada orden descartando los finalizados y pendientes
            int usedSlots = activeOrders
                .SelectMany(o => o.Items)
                .Count(i => i.Status == ItemStatus.Preparing);


            // lo compara con la cantidad maxima de platos que puede operar la cocina
            if (usedSlots >= maxDishes)
                return; // cocina llena

            var availableSlots = maxDishes - usedSlots;

            var order = await _repository // trae la siguiente orden en espera 
                .GetNextWaitingOrderAsync();

            if (order == null)
                return;

            int itemCount = order.Items.Count;

            if (itemCount > availableSlots) // si los items de la orden no entran en los slots disponibles 
                return; // no hay espacio para esta orden

            CalculateSyncTimes(order);

            order.Status = OrderStatus.Preparing; // marco la orden en preparacion 

            await _repository.UpdateAsync(order);

        }


        // Motor de Orquestación run ruuun: calcula cuándo empezar cada plato
        // para que todos terminen al mismo tiempo (sincronización de mesa).
        private void CalculateSyncTimes(KitchenOrder order)
        {
            if (order == null || order.Items == null || !order.Items.Any())
                return;

            // Paso 1: calcular tiempo total de cada item considerando cantidad + multiplicador
            foreach (var item in order.Items)
            {
                item.EstimatedTime = (int)Math.Round(
                    item.DurationMinutes *
                    (1 + (item.Quantity - 1) * (decimal)item.FactorMultiplierTime)
                );
            }

            // Paso 2: encontrar el item más lento (define duración total de la orden)
            var maxTime = order.Items.Max(i => i.EstimatedTime);

            var targetFinishTime = DateTime.UtcNow.AddMinutes(maxTime);

            // Paso 3: sincronizar todos los items hacia atrás desde el final de la orden
            foreach (var item in order.Items)
            {
                item.StartTime = targetFinishTime.AddMinutes(-item.EstimatedTime);
                item.FinishTime = targetFinishTime;

                item.Status = ItemStatus.Preparing;
            }


        }

        // checkea si la  orden esta completa , la marca como lista y retorna true  
        private async Task<bool> CheckAndCompleteOrderAsync(Guid kitchenOrderId)
        {
            var order = await _repository.GetByIdWithItemsAsync(kitchenOrderId);

            if (order == null)
                return false;

            bool allItemsCompleted = order.Items
                .All(i => i.Status == ItemStatus.Finished);

            if (!allItemsCompleted)
                return false;

            order.Status = OrderStatus.Ready;
            order.ActualFinishTime = DateTime.UtcNow;
            order.LastUpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(order);

            return true;
        }


    }
}
