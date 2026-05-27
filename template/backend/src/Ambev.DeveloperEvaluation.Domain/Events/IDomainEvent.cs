using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public interface IDomainEvent : INotification
{
    DateTime OccurredAt { get; }
}
