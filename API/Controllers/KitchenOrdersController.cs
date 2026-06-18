using System.Threading.Tasks;
using Application.Interfaces;
using Application.UseCases.Handlers;
using Application.UseCases.KitchenOrders.Comands;
using Application.UseCases.KitchenOrders.Handlers;
using Application.UseCases.KitchenOrders.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KitchenOrdersController : ControllerBase
    {
        private readonly ICreateKitchenOrderHandler _createHandler;
        private readonly IRecalculateOrderHandler _recalculateHandler;
        private readonly GetKitchenQueueHandler _getKitchenQueueHandler;
        private readonly IGetKitchenOrderByIdHandler _getByIdHandler;
        private readonly IGetKitchenOrderByOrderIdHandler _getByOrderIdHandler;
        private readonly IMarkAsDeliveredHandler _markAsDeliveredHandler;

        public KitchenOrdersController(ICreateKitchenOrderHandler createHandler, IRecalculateOrderHandler recalculateHandler, IGetKitchenOrderByIdHandler getByIdHandler,IGetKitchenOrderByOrderIdHandler getByOrderIdHandler,IMarkAsDeliveredHandler markAsDeliveredHandler, GetKitchenQueueHandler getKitchenQueueHandler)
        {
            _createHandler = createHandler;
            _recalculateHandler = recalculateHandler;
            _getByIdHandler = getByIdHandler;
            _getByOrderIdHandler = getByOrderIdHandler;
            _markAsDeliveredHandler = markAsDeliveredHandler;
            _getKitchenQueueHandler = getKitchenQueueHandler;
        }

        /// Obtiene el detalle completo de una orden de cocina por su ID interno.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetKitchenOrderByIdQuery { Id = id };
            var order = await _getByIdHandler.GetKitchenOrderById(query);
            return Ok(order);
        }

        /// Busca una orden de cocina por el OrderId del Order Service (referencia externa).
        [HttpGet("by-order/{orderId}")]
        public async Task<IActionResult> GetByOrderId(Guid orderId)
        {
            var query = new GetKitchenOrderByOrderIdQuery { OrderId = orderId };
            var order = await _getByOrderIdHandler.GetKitchenOrderByOrderId(query);
            return Ok(order);
        }

        /// Marca una orden como entregada al mozo. Solo disponible cuando la orden está en estado 'Ready'.
        [HttpPut("{id}/deliver")]
        public async Task<IActionResult> MarkAsDelivered(Guid id)
        {
            var command = new MarkAsDeliveredCommand { Id = id };
            var order = await _markAsDeliveredHandler.MarkAsDelivered(command);
            return Ok(order);
        }

        [HttpGet("queue")]
        public async Task<IActionResult> GetQueue()
        {
            var queue = await _getKitchenQueueHandler.HandleAsync();

            return Ok(queue);
        }

        [HttpPut("items/{itemId}/start")]
        public async Task<IActionResult> StartItemPreparation(Guid itemId, [FromServices] StartItemPreparationHandler handler)
        {
            var success = await handler.HandleAsync(itemId);

            if (!success)
            {
                return NotFound(new { message = "No se encontró el plato o la orden." });
            }

            return Ok(new { message = "Preparación del plato iniciada correctamente." });
        }

        [HttpPut("items/{itemId}/complete")]
        public async Task<IActionResult> CompleteItem(Guid itemId, [FromServices] CompleteItemHandler handler)
        {
            var success = await handler.HandleAsync(itemId);

            if (!success)
            {
                return NotFound(new { message = "No se encontró el plato o la orden." });
            }

            return Ok(new { message = "Plato marcado como listo. Orden actualizada." });
        }

        [HttpPut("items/{itemId}/cancel")]
        public async Task<IActionResult> CancelItem(Guid itemId, [FromServices] CancelItemHandler handler)
        {
            var success = await handler.HandleAsync(itemId);

            if (!success)
            {
                return NotFound(new { message = "No se encontró el plato o ya estaba cancelado." });
            }

            return Ok(new { message = "Plato cancelado. Orden recalculada correctamente." });
        }

        /// Crea una nueva orden de cocina con sincronización automática de tiempos.
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateKitchenOrderCommand command)
        {
            var order = await _createHandler.CreateKitchenOrder(command);
            return Ok(order);
        }

        /// Recalcula los tiempos de una orden ante un imprevisto (plato arruinado o cancelado).
        [HttpPut("{orderId}/recalculate")]
        public async Task<IActionResult> Recalculate(
        Guid orderId,
        [FromBody] RecalculateOrderCommand command)
        {
            
            command.OrderId = orderId;

            var order = await _recalculateHandler.RecalculateOrder(command);
            return Ok(order);
        }
    }
}
