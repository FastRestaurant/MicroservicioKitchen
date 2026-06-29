using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class KitchenOrderRepository : IKitchenOrderRepository
{
    private readonly ApplicationDbContext _context;

    public KitchenOrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<KitchenOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.KitchenOrders
            .Include(k => k.Items)
            .FirstOrDefaultAsync(k => k.Id == id, cancellationToken);
    }

    public async Task<KitchenOrder?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.KitchenOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.OrderId == orderId, cancellationToken);
    }

    public async Task<KitchenOrder?> GetByOrderIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.KitchenOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
    }

    public async Task<KitchenOrder> CreateAsync(KitchenOrder order, CancellationToken cancellationToken = default)
    {
        await _context.KitchenOrders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task AddItemsAsync(IEnumerable<KitchenOrderItem> items, CancellationToken cancellationToken = default)
    {
        await _context.KitchenOrderItems.AddRangeAsync(items, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<KitchenOrder> UpdateAsync(KitchenOrder order, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return order;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException();
        }
    }

    public async Task<List<KitchenOrder>> GetActiveOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.KitchenOrders
            .Include(o => o.Items)
            .Where(o => o.Status == OrderStatus.Preparing)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountActivePreparingItemsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _context.KitchenOrderItems
            .Where(i => i.Status == ItemStatus.Preparing &&
                        i.StartTime != null &&
                        i.StartTime <= now &&
                        i.Order.Status == OrderStatus.Preparing)
            .CountAsync(cancellationToken);
    }

    public async Task<int> CountPreparingItemsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.KitchenOrderItems
            .Where(i => i.Status == ItemStatus.Preparing &&
                        i.Order.Status == OrderStatus.Preparing)
            .CountAsync(cancellationToken);
    }

    public async Task<KitchenOrder?> GetNextUpcomingOrderAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _context.KitchenOrders
            .Include(o => o.Items)
            .Where(o => o.Status == OrderStatus.Preparing)
            .Where(o => o.Items.Any(i => i.Status == ItemStatus.Preparing && i.StartTime != null && i.StartTime > now))
            .Where(o => !o.Items.Any(i => i.Status == ItemStatus.Preparing && i.StartTime != null && i.StartTime <= now))
            .OrderBy(o => o.Items
                .Where(i => i.Status == ItemStatus.Preparing && i.StartTime != null && i.StartTime > now)
                .Min(i => i.StartTime))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<KitchenOrder?> GetNextWaitingOrderAsync(CancellationToken cancellationToken = default)
    {
        return await _context.KitchenOrders
            .Include(o => o.Items)
            .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Preparing)
            .Where(o => o.Items.Any(i => i.Status == ItemStatus.Pending))
            .OrderBy(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<KitchenOrder?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.KitchenOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }
}
