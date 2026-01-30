using Business.Interfaces.Repositories;
using Domain.Events;
using MediatR;

namespace Business.Handlers.Items.Events.ItemPriceHistoryChangedEvent
{
    public class ItemPriceHistoryChangedHandler(
        IUnitOfWork unitOfWork) : INotificationHandler<ItemPriceChangedEvent>
    {
        public Task Handle(ItemPriceChangedEvent notification, CancellationToken cancellationToken)
        {
            // TODO: add handler here 
            return Task.CompletedTask;
        }
    }
}
