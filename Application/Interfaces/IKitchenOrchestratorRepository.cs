using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IKitchenOrchestratorRepository
    {
        Task<int> GetMaxConcurrentDishesAsync();
        Task<List<KitchenOrderItem>> GetFlatActiveQueueAsync(int limit);
    }
}
