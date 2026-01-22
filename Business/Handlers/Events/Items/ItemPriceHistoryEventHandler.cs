using Business.Interfaces.Repositories;
using Domain.Events;
using MediatR;

namespace Business.Handlers.Events.Items;

public class ItemPriceHistoryEventHandler(
    IUnitOfWork unitOfWork) : INotificationHandler<ItemPriceChangedEvent>
{
    public Task Handle(ItemPriceChangedEvent notification, CancellationToken cancellationToken)
    {
        // TODO: add handler here 
        return Task.CompletedTask;
    }
}