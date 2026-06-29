using Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Net;

namespace Infrastructure.Service;

public sealed class OrderServiceClient : IOrderServiceClient
{
    private const string KitchenReadyPath = "api/v1/orders/{0}/kitchen-ready";
    private const string OrderItemStatusPath = "api/v1/orders/{0}/items/{1}/status";

    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderServiceClient> _logger;

    public OrderServiceClient(HttpClient httpClient, ILogger<OrderServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task NotifyOrderItemReadyAsync(Guid orderId, Guid orderItemId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync(
                string.Format(OrderItemStatusPath, orderId, orderItemId),
                new UpdateOrderItemStatusRequest("Ready"),
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("OrderService no encontro la orden {OrderId} o item {OrderItemId} al notificar item listo.", orderId, orderItemId);
                return;
            }

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "No se pudo notificar a OrderService que el item {OrderItemId} de la orden {OrderId} esta listo.", orderItemId, orderId);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Timeout al notificar a OrderService que el item {OrderItemId} de la orden {OrderId} esta listo.", orderItemId, orderId);
        }
    }

    public async Task NotifyOrderReadyAsync(Guid orderId, bool wasDelayed, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                string.Format(KitchenReadyPath, orderId),
                new KitchenReadyRequest(wasDelayed),
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("OrderService no encontro la orden {OrderId} al notificar kitchen-ready.", orderId);
                return;
            }

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "No se pudo notificar a OrderService que la orden de cocina {OrderId} esta lista.", orderId);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Timeout al notificar a OrderService que la orden de cocina {OrderId} esta lista.", orderId);
        }
    }

    private sealed record KitchenReadyRequest(bool WasDelayed);
    private sealed record UpdateOrderItemStatusRequest(string NewStatus);
}
