using MediatR;

namespace Domain.Events
{
    public record ItemPriceChangedEvent(
      Guid ItemId,
      decimal OldPrice,
      decimal NewPrice,
      DateTime OccurredOn) : INotification;
}