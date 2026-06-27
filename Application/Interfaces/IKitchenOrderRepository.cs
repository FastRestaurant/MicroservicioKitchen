using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IKitchenOrderRepository
    {
        Task<KitchenOrder?> GetByIdAsync(Guid id);
        Task<KitchenOrder> CreateAsync(KitchenOrder order);
        Task<KitchenOrder> UpdateAsync(KitchenOrder order);
        Task<KitchenOrder?> GetOrderByItemIdAsync(Guid itemId);
        Task<List<KitchenOrder>> GetActiveOrdersAsync();
        Task<KitchenOrder?> GetNextWaitingOrderAsync();
        Task<KitchenOrder?> GetByIdWithItemsAsync(Guid id);

    }
}
