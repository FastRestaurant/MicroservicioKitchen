using System.Threading.Tasks;
using Application.Interfaces;
using Application.UseCases.KitchenOrders.Comands;
using Application.UseCases.KitchenOrders.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{

    [ApiController]
    [Route("api/kitchenOrders")]
    public class KitchenOrdersController : ControllerBase
    {
        private readonly ICreateKitchenOrderHandler _createHandler;
        private readonly IKitchenOrchestrator _orchestrator;
        private readonly ICompleteKitchenOrderItemHandler _completeItemHandler;
        public KitchenOrdersController(ICreateKitchenOrderHandler createHandler, IKitchenOrchestrator orchestrator, ICompleteKitchenOrderItemHandler completeItemHandler)
        {
            _createHandler = createHandler;
            _orchestrator = orchestrator;
            _completeItemHandler = completeItemHandler;
        }




        // crea la orden en kitchen  
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateKitchenOrderCommand command)
        {
            var order = await _createHandler.CreateKitchenOrder(command);
            return Ok(order);
        }




        // devolver la lista de platos al front 
        [HttpGet("queue")]
        public async Task<IActionResult> GetQueue()
        {
            var queue = await _orchestrator.GetItemsFromQueueAsync();
            return Ok(queue);
        }


        // para marcar plato como ya finalizado 
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("ok");
        }

        // para marcar un plato como ya finalizado
        [HttpPut("items/{id}/complete")]
        public async Task<IActionResult> CompleteItem(Guid id)
        {
            var command = new CompleteKitchenOrderItemCommand { ItemId = id };
            await _completeItemHandler.ExecuteAsync(command);
            return NoContent();
        }

    }
}
