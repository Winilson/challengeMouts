using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

public class SaleCancelledEventHandler : INotificationHandler<SaleCancelledEvent>
{
    private readonly ILogger<SaleCancelledEventHandler> _logger;
    public SaleCancelledEventHandler(ILogger<SaleCancelledEventHandler> logger) => _logger = logger;

    public Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        var sale = notification.Sale;
        _logger.LogWarning(
            "[DomainEvent] SaleCancelled | SaleId={SaleId} | SaleNumber={SaleNumber} | " +
            "Customer={CustomerName} | RefundAmount={Amount:F2} | OccurredAt={OccurredAt:o}",
            sale.Id, sale.SaleNumber, sale.CustomerName, sale.TotalAmount, notification.OccurredAt);
        return Task.CompletedTask;
    }
}
