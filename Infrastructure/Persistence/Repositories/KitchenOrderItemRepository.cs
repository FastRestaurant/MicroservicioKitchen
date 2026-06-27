using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Application.DTOs;

namespace Infrastructure.Persistence.Repositories
{
    public class KitchenOrderItemRepository : IKitchenOrderItemRepository
    {
        private readonly ApplicationDbContext _context;

        public KitchenOrderItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<KitchenOrderItem?> GetItemByIdAsync(Guid itemId)
        {
            return await _context.KitchenOrderItems
                .FirstOrDefaultAsync(i => i.Id == itemId);
        }

        public async Task UpdateItemAsync(KitchenOrderItem item)
        {
            _context.KitchenOrderItems.Update(item);
            await _context.SaveChangesAsync();
        }
        public async Task<int> GetActiveCountAsync()
        {
            return await _context.KitchenOrderItems
                .Where(x => x.Status == Domain.Enums.ItemStatus.Preparing)
                .CountAsync();
        }
        public async Task<List<KitchenOrderItem>> GetNextWaitingItemsAsync(int take)
        {
            return await _context.KitchenOrderItems
                .Include(i => i.Order)
                .Where(i => i.Status == Domain.Enums.ItemStatus.Pending)
                .OrderBy(i => i.Order.CreatedAt)   // 👈 clave
                .ThenBy(i => i.Id)                 // desempate estable
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<KitchenOrderItem>> GetItemsReadyToCookAsync()
        {
            var now = DateTime.UtcNow;

            return await _context.KitchenOrderItems
                .Where(i => i.Status == ItemStatus.Preparing &&
                            i.StartTime <= now)
                .OrderBy(i => i.StartTime)
                .ToListAsync();
        }
        public async Task<List<KitchenOrderItem>> GetItemsReadyToWaitingAsync()
        {
            var now = DateTime.UtcNow;

            return await _context.KitchenOrderItems
                .Include(i => i.Order)
                .Where(i =>
                    (i.Order.Status == OrderStatus.Preparing &&
                     i.Status == ItemStatus.Preparing &&
                     i.StartTime > now)
                    ||
                    (i.Order.Status == OrderStatus.Pending &&
                     i.Status == ItemStatus.Preparing))
                .OrderBy(i => i.Order.Status == OrderStatus.Preparing ? 0 : 1)
                .ThenBy(i => i.Order.Status == OrderStatus.Preparing
                    ? i.StartTime
                    : i.Order.CreatedAt)
                .ToListAsync();
        }

    }
}
