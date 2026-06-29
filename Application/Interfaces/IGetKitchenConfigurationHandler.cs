using Application.DTOs;

namespace Application.Interfaces;

public interface IGetKitchenConfigurationHandler
{
    Task<KitchenConfigurationDto> ExecuteAsync(CancellationToken cancellationToken = default);
}
