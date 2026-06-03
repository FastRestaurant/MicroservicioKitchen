using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
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

        public async Task<List<KitchenOrder>> GetActiveOrdersAsync()
        {
            return await _context.KitchenOrders
                .Include(k => k.Items)
                .Where(k => k.Status != Domain.Enums.OrderStatus.Delivered &&
                           k.Status != Domain.Enums.OrderStatus.Cancelled)
                .OrderBy(k => k.CreatedAt)
                .ToListAsync();
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
    }
}
