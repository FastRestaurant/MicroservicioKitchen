using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class KitchenOrderItemRepository : IKitchenOrderItemRepository
{
    private readonly ApplicationDbContext _context;

    public KitchenOrderItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<KitchenOrderItem?> GetItemByIdAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await _context.KitchenOrderItems
            .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);
    }

    public async Task UpdateItemAsync(KitchenOrderItem item, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.KitchenOrderItems.Update(item);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException();
        }
    }

    public async Task<List<KitchenOrderItem>> GetItemsReadyToCookAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _context.KitchenOrderItems
            .AsNoTracking()
            .Include(i => i.Order)
            .Where(i => i.Status == ItemStatus.Preparing &&
                        i.Order.Status != OrderStatus.Cancelled &&
                        i.StartTime != null && i.StartTime <= now)
            .OrderBy(i => i.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<KitchenOrderItem>> GetItemsToWaitingAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _context.KitchenOrderItems
            .AsNoTracking()
            .Include(i => i.Order)
            .Where(i =>
                i.Order.Status != OrderStatus.Cancelled &&
                i.Status == ItemStatus.Preparing &&
                i.StartTime > now)
            .OrderBy(i => i.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<KitchenOrderItem>> GetPendingItemsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.KitchenOrderItems
            .AsNoTracking()
            .Include(i => i.Order)
            .Where(i =>
                i.Order.Status != OrderStatus.Cancelled &&
                i.Status == ItemStatus.Pending)
            .OrderBy(i => i.Order.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
