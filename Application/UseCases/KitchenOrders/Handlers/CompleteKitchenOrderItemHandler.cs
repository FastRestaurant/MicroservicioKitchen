using Application.Interfaces;
using Application.UseCases.KitchenOrders.Comands;
using Domain.Enums;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.KitchenOrders.Handlers
{
    public class CompleteKitchenOrderItemHandler : ICompleteKitchenOrderItemHandler
    {
        private readonly IKitchenOrchestrator _kitchenOrchestrator;

        public CompleteKitchenOrderItemHandler(IKitchenOrchestrator kitchenOrchestrator)
        {
            _kitchenOrchestrator = kitchenOrchestrator;
        }

        public async Task ExecuteAsync(Guid id)
        {
            await _kitchenOrchestrator.FinishItemAsync(id);

        }
    }
}
