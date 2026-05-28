using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

public class SaleModifiedEventHandler : INotificationHandler<SaleModifiedEvent>
{
    private readonly ILogger<SaleModifiedEventHandler> _logger;
    public SaleModifiedEventHandler(ILogger<SaleModifiedEventHandler> logger) => _logger = logger;

    public Task Handle(SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        var sale = notification.Sale;
        _logger.LogInformation(
            "[DomainEvent] SaleModified | SaleId={SaleId} | SaleNumber={SaleNumber} | " +
            "Customer={CustomerName} | Branch={BranchName} | OccurredAt={OccurredAt:o}",
            sale.Id, sale.SaleNumber, sale.CustomerName, sale.BranchName, notification.OccurredAt);
        return Task.CompletedTask;
    }
}
