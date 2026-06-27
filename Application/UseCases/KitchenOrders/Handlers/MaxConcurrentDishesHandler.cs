using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.UseCases.KitchenOrders.Comands;
using Domain.Exceptions;

namespace Application.UseCases.KitchenOrders.Handlers
{
    public class MaxConcurrentDishesHandler : IMaxConcurrentDishesHandler
    {
        private readonly IKitchenOrchestratorRepository _orchestratorRepository;

        public MaxConcurrentDishesHandler(IKitchenOrchestratorRepository orchestratorRepository)
        {
            _orchestratorRepository = orchestratorRepository;
        }

        public async Task ExecuteAsync(UpdateMaxConcurrentDishesCommand command)
        {
            if (command.MaxConcurrentDishes <= 0)
                throw new ValidationExceptions(new Dictionary<string, string[]>
        {
            { "MaxConcurrentDishes", new[] { "MaxConcurrentDishes must be greater than 0." } }
        });

            await _orchestratorRepository.UpdateMaxConcurrentDishesAsync(command.MaxConcurrentDishes);
        }
    }
}
