using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class KitchenOrderRepository : IKitchenOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public KitchenOrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<KitchenOrder?> GetByIdAsync(Guid id)
        {
            return await _context.KitchenOrders
                .Include(k => k.Items)
                .FirstOrDefaultAsync(k => k.Id == id);
        }

        public async Task<KitchenOrder> CreateAsync(KitchenOrder order)
        {
            await _context.KitchenOrders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<KitchenOrder> UpdateAsync(KitchenOrder order)
        {
            _context.KitchenOrders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<KitchenOrder?> GetOrderByItemIdAsync(Guid itemId)
        {
            return await _context.KitchenOrders
                .Include(k => k.Items)
                .FirstOrDefaultAsync(k => k.Items.Any(i => i.Id == itemId));
        }

        public async Task<List<KitchenOrder>> GetActiveOrdersAsync()
        {
            return await _context.KitchenOrders
                .Include(o => o.Items)
                .Where(o => o.Status == Domain.Enums.OrderStatus.Preparing)
                .ToListAsync();
        }

        public async Task<KitchenOrder?> GetNextWaitingOrderAsync()
        {
            return await _context.KitchenOrders
                .Include(o => o.Items)
                .Where(o => o.Status == OrderStatus.Pending)
                .OrderBy(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<KitchenOrder?> GetByIdWithItemsAsync(Guid id)
        {
            return await _context.KitchenOrders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
        }



    }
}
