using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public class ItemCancelledEvent : IDomainEvent
{
    public Sale Sale { get; }
    public SaleItem Item { get; }
    public DateTime OccurredAt { get; }

    public ItemCancelledEvent(Sale sale, SaleItem item)
    {
        Sale = sale;
        Item = item;
        OccurredAt = DateTime.UtcNow;
    }
}
