using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IKitchenOrderItemRepository
    {
        Task UpdateItemAsync(KitchenOrderItem item);
        Task<int> GetActiveCountAsync();
        Task<List<KitchenOrderItem>> GetItemsReadyToCookAsync();
        Task<KitchenOrderItem?> GetItemByIdAsync(Guid itemId);
        Task<List<KitchenOrderItem>> GetItemsReadyToWaitingAsync();
    }
}
