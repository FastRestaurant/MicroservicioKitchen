using System.Reflection.Metadata;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.KitchenOrders.Comands;
using Application.UseCases.KitchenOrders.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{

    [ApiController] 
    [Route("api/kitchenOrders")]
    [Authorize]
    public class KitchenOrdersController : ControllerBase
    {
        private readonly IKitchenOrchestrator _orchestrator;
        private readonly ICreateKitchenOrderHandler _createHandler;
        private readonly ICancelKitchenOrderHandler _cancelKitchenOrderHandler;
        private readonly ICompleteKitchenOrderItemHandler _completeItemHandler;
        private readonly IMaxConcurrentDishesHandler _maxConcurrentDishesHandler;
        public KitchenOrdersController( ICreateKitchenOrderHandler createHandler,
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


        // crea la orden en kitchen  
        [HttpPost]
        [Authorize(Roles = "Admin,Waitress")]
        public async Task<IActionResult> Create([FromBody] CreateKitchenOrderCommand command)
        {
            var order = await _createHandler.CreateKitchenOrder(command);
            return Ok(order);
        }



        // devolver la lista de platos actuales al front 
        [HttpGet("queue")]
        [Authorize(Roles = "Admin,Kitchen")]
        public async Task<ActionResult<List<KitchenQueueItemResponse>>> GetQueue()
        {
            return Ok(await _orchestrator.GetItemsFromQueueAsync());
        }

        // devolver la lista de platos en espera al front 
        [HttpGet("queue-waiting-items")]
        [Authorize(Roles = "Admin,Kitchen")]
        public async Task<ActionResult<List<KitchenQueueItemResponse>>> GetWaitingItems()
        {
            return Ok(await _orchestrator.GetWaitingItemsAsync());
        }


        // para marcar un plato como ya finalizado
        [HttpPatch("items/{id}/complete")]
        [Authorize(Roles = "Admin,Kitchen")]
        public async Task<IActionResult> CompleteItem(Guid id)
        {
            await _completeItemHandler.ExecuteAsync(id);
            return NoContent();
        }

        // cancela una order con su id siempre y cuando no alla entrado a la cocina
        [HttpPatch("orders/{id}/cancel")]
        [Authorize(Roles = "Admin,Waitress")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            await _cancelKitchenOrderHandler.ExecuteAsync(id);
            return NoContent();
        }

        // configura el valor maximo  de platos que se pueden trabajar en cocina 
        [HttpPatch("max-concurrent-dishes")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMaxConcurrentDishes(UpdateMaxConcurrentDishesCommand command)
        {
            await _maxConcurrentDishesHandler.ExecuteAsync(command);
            return NoContent();
        }

    }
}
