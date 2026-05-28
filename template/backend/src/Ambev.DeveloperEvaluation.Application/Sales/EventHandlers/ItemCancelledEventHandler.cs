using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

public class ItemCancelledEventHandler : INotificationHandler<ItemCancelledEvent>
{
    private readonly ILogger<ItemCancelledEventHandler> _logger;
    public ItemCancelledEventHandler(ILogger<ItemCancelledEventHandler> logger) => _logger = logger;

    public Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        var sale = notification.Sale;
        var item = notification.Item;
        _logger.LogInformation(
            "[DomainEvent] ItemCancelled | SaleId={SaleId} | SaleNumber={SaleNumber} | " +
            "ItemId={ItemId} | Product={ProductName} | Quantity={Quantity} | " +
            "RefundAmount={Amount:F2} | OccurredAt={OccurredAt:o}",
            sale.Id, sale.SaleNumber, item.Id, item.ProductName, item.Quantity,
            item.TotalAmount, notification.OccurredAt);
        return Task.CompletedTask;
    }
}
