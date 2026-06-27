using Application.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IKitchenOrchestrator
    {
        Task EnqueueOrderAsync(Guid kitchenOrderId);
        Task<List<KitchenQueueItemResponse>> GetItemsFromQueueAsync();
        Task FinishItemAsync(Guid itemId);
        Task<List<KitchenQueueItemResponse>> GetWaitingItemsAsync();

    }
}
