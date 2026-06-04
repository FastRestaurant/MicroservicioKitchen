using Application.UseCases.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KitchenController : ControllerBase
    {
        private readonly GetKitchenQueueHandler _getKitchenQueueHandler;

        public KitchenController(GetKitchenQueueHandler getKitchenQueueHandler)
        {
            _getKitchenQueueHandler = getKitchenQueueHandler;
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
    }
}
