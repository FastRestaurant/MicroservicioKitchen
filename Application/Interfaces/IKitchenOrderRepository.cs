using Domain.Entities;

namespace Application.Interfaces;

public interface IKitchenOrderRepository
{
    Task<KitchenOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<KitchenOrder?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<KitchenOrder?> GetByOrderIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<KitchenOrder> CreateAsync(KitchenOrder order, CancellationToken cancellationToken = default);
    Task AddItemsAsync(IEnumerable<KitchenOrderItem> items, CancellationToken cancellationToken = default);
    Task<KitchenOrder> UpdateAsync(KitchenOrder order, CancellationToken cancellationToken = default);
    Task<List<KitchenOrder>> GetActiveOrdersAsync(CancellationToken cancellationToken = default);
    Task<int> CountPreparingItemsAsync(CancellationToken cancellationToken = default);
    Task<int> CountActivePreparingItemsAsync(CancellationToken cancellationToken = default);
    Task<KitchenOrder?> GetNextUpcomingOrderAsync(CancellationToken cancellationToken = default);
    Task<KitchenOrder?> GetNextWaitingOrderAsync(CancellationToken cancellationToken = default);
    Task<KitchenOrder?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
}
