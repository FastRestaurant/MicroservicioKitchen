using API.Authorization;
using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.KitchenOrders.Comands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/kitchenOrders")]
public sealed class KitchenOrdersController : ControllerBase
{
    private readonly IKitchenOrchestrator _orchestrator;
    private readonly ICreateKitchenOrderHandler _createHandler;
    private readonly ICancelKitchenOrderHandler _cancelKitchenOrderHandler;
    private readonly ICompleteKitchenOrderItemHandler _completeItemHandler;
    private readonly IMaxConcurrentDishesHandler _maxConcurrentDishesHandler;

    public KitchenOrdersController(
        ICreateKitchenOrderHandler createHandler,
        ICancelKitchenOrderHandler cancelKitchenOrder,
        IKitchenOrchestrator orchestrator,
        ICompleteKitchenOrderItemHandler completeItemHandler,
        IMaxConcurrentDishesHandler maxConcurrentDishesHandler)
    {
        _createHandler = createHandler;
        _orchestrator = orchestrator;
        _completeItemHandler = completeItemHandler;
        _cancelKitchenOrderHandler = cancelKitchenOrder;
        _maxConcurrentDishesHandler = maxConcurrentDishesHandler;
    }

    [HttpPost]
    [Authorize(Roles = ApplicationRoles.AdminWaitressOrKitchen)]
    public async Task<ActionResult<CreateKitchenOrderResponseDto>> Create([FromBody] CreateKitchenOrderCommand command, CancellationToken cancellationToken)
        => Ok(await _createHandler.CreateKitchenOrder(command, cancellationToken));

    [HttpGet("queue")]
    [Authorize(Roles = ApplicationRoles.AdminOrKitchen)]
    public async Task<ActionResult<List<KitchenQueueItemResponse>>> GetQueue(CancellationToken cancellationToken)
        => Ok(await _orchestrator.GetItemsFromQueueAsync(cancellationToken));

    [HttpGet("queue-waiting-items")]
    [Authorize(Roles = ApplicationRoles.AdminOrKitchen)]
    public async Task<ActionResult<List<KitchenQueueItemResponse>>> GetWaitingItems(CancellationToken cancellationToken)
        => Ok(await _orchestrator.GetWaitingItemsAsync(cancellationToken));

    [HttpPatch("items/{id:guid}/complete")]
    [Authorize(Roles = ApplicationRoles.AdminOrKitchen)]
    public async Task<IActionResult> CompleteItem(Guid id, CancellationToken cancellationToken)
    {
        await _completeItemHandler.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("orders/{id:guid}/cancel")]
    [Authorize(Roles = ApplicationRoles.AdminOrWaitress)]
    public async Task<IActionResult> CancelOrder(Guid id, CancellationToken cancellationToken)
    {
        await _cancelKitchenOrderHandler.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("max-concurrent-dishes")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> UpdateMaxConcurrentDishes([FromBody] UpdateMaxConcurrentDishesCommand command, CancellationToken cancellationToken)
    {
        await _maxConcurrentDishesHandler.ExecuteAsync(command, cancellationToken);
        return NoContent();
    }
}
