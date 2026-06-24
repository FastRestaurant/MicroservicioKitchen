using Application.DTOs;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.service
{
    public class KitchenOrchestrator : IKitchenOrchestrator
    {
        private readonly IKitchenOrchestratorRepository _orchestratorRepository;

        public KitchenOrchestrator(IKitchenOrchestratorRepository orchestratorRepository)
        {
            _orchestratorRepository = orchestratorRepository;
        }

        public async Task EnqueueOrderAsync(Guid kitchenOrderId)
        {
            // Lógica futura de encolamiento...
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
    }
}
