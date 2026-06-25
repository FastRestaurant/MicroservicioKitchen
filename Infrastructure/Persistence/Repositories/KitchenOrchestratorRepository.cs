using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class KitchenOrchestratorRepository : IKitchenOrchestratorRepository
    {
        private readonly ApplicationDbContext _context;

        public KitchenOrchestratorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetMaxConcurrentDishesAsync()
        {
            //valor por defecto seguro 10
            var config = await _context.KitchenConfigurations.FirstOrDefaultAsync();
            return config?.MaxConcurrentDishes ?? 10;
        }

        public async Task<List<KitchenOrderItem>> GetFlatActiveQueueAsync(int limit)
        {
            return await _context.KitchenOrderItems
                .Include(i => i.Order)
                .Where(i => i.Status == ItemStatus.Pending || i.Status == ItemStatus.Preparing)
                .OrderBy(i => i.StartTime)
                .Take(limit)
                .ToListAsync();
        }
    }
}
