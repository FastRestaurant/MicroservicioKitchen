using Application.DTOs;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain.Entities;


namespace Application.service
{
    public class KitchenOrchestrator : IKitchenOrchestrator
    {

        private readonly IKitchenOrderRepository _repository;
        private readonly IKitchenOrchestratorRepository _orchestratorRepository;
        public KitchenOrchestrator(IKitchenOrderRepository repository, IKitchenOrchestratorRepository orchestratorRepository)
        {
            _repository = repository;
            _orchestratorRepository = orchestratorRepository;
        }
        public async Task EnqueueOrderAsync(Guid kitchenOrderId)
        {
            // Buscar la orden en la base de datos
            var order = await _repository.GetByIdAsync(kitchenOrderId);

            if (order == null)
                throw new Exception($"No se encontró la orden con ID {kitchenOrderId}");

            // Calcular los tiempos de sincronización
            CalculateSyncTimes(order.Items.ToList());

            // Calcular el tiempo estimado de finalización de toda la orden
            var maxTime = order.Items.Max(i => i.EstimatedTime);
            order.EstimatedFinishTime = DateTime.UtcNow.AddMinutes(maxTime);
            order.LastUpdatedAt = DateTime.UtcNow;

            // Guardar los cambios
            await _repository.UpdateAsync(order);
        }

        public async Task<List<KitchenQueueItemDto>> GetItemsFromQueueAsync()
        {
            // 1. Obtener la capacidad máxima configurada
            int maxDishes = await _orchestratorRepository.GetMaxConcurrentDishesAsync();

            // 2. Traer los ítems planos ordenados y limitados
            var activeItems = await _orchestratorRepository.GetFlatActiveQueueAsync(maxDishes);

            // 3. Mapear de Entidad de Dominio a DTO Plano para el Frontend
            return activeItems.Select(item => new KitchenQueueItemDto
            {
                ItemId = item.Id,
                KitchenOrderId = item.KitchenOrderId,
                TableNumber = item.Order?.TableNumber ?? 0,
                ProductName = item.ProductName,
                EstimatedTime = item.EstimatedTime,
                StartTime = item.StartTime,
                Status = item.Status.ToString(),
                PriorityScore = item.PriorityScore,
                Notes = item.Notes,
                IsRushed = item.IsRushed
            }).ToList();
        }


        /// Motor de Orquestación: calcula cuándo empezar cada plato
        /// para que todos terminen al mismo tiempo (sincronización de mesa).
        private void CalculateSyncTimes(List<KitchenOrderItem> items)
        {
            if (!items.Any()) return;

            // Paso 1: Encontrar el plato más lento (el que más tiempo tarda)
            var maxTime = items.Max(i => i.EstimatedTime);

            // Paso 2: Calcular el tiempo objetivo de finalización
            // Todos los platos de la mesa deben estar listos en este momento
            var targetFinishTime = DateTime.UtcNow.AddMinutes(maxTime);

            // Paso 3: Asignar StartTime y PriorityScore a cada item
            foreach (var item in items)
            {
                // El StartTime es: Target - lo que tarda mi plato
                // Ej: Si el target es 20:15 y mi plato tarda 8 min → empiezo a las 20:07
                item.StartTime = targetFinishTime.AddMinutes(-item.EstimatedTime);

                // a más tiempo, más prioridad
                // Esto hace que los platos lentos aparezcan primero en el KDS
                item.PriorityScore = item.EstimatedTime;
            }
        }

    }
}
