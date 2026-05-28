using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

public class SaleCreatedEventHandler : INotificationHandler<SaleCreatedEvent>
{
    private readonly ILogger<SaleCreatedEventHandler> _logger;

    public SaleCreatedEventHandler(ILogger<SaleCreatedEventHandler> logger) => _logger = logger;

    public Task Handle(SaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        var sale = notification.Sale;

        _logger.LogInformation(
            "[DomainEvent] SaleCreated | SaleId={SaleId} | SaleNumber={SaleNumber} | " +
            "Customer={CustomerName} | Branch={BranchName} | Items={ItemCount} | " +
            "TotalAmount={TotalAmount:F2} | OccurredAt={OccurredAt:o}",
            sale.Id, sale.SaleNumber, sale.CustomerName, sale.BranchName,
            sale.Items.Count, sale.TotalAmount, notification.OccurredAt);

        return Task.CompletedTask;
    }
}
