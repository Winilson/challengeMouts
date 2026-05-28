using Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.EventHandlers;

public class SaleEventHandlersTests
{
    [Fact(DisplayName = "SaleCreatedEventHandler manipula sem lançar exceção")]
    public async Task SaleCreated_Handles()
    {
        var handler = new SaleCreatedEventHandler(Substitute.For<ILogger<SaleCreatedEventHandler>>());
        var sale = SaleTestData.GenerateSaleWithItems(2);
        Func<Task> act = () => handler.Handle(new SaleCreatedEvent(sale), CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "SaleModifiedEventHandler handles without throwing")]
    public async Task SaleModified_Handles()
    {
        var handler = new SaleModifiedEventHandler(Substitute.For<ILogger<SaleModifiedEventHandler>>());
        var sale = SaleTestData.GenerateValidSale();
        Func<Task> act = () => handler.Handle(new SaleModifiedEvent(sale), CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "SaleModifiedEventHandler manipula sem lançar exceção")]
    public async Task SaleCancelled_Handles()
    {
        var handler = new SaleCancelledEventHandler(Substitute.For<ILogger<SaleCancelledEventHandler>>());
        var sale = SaleTestData.GenerateValidSale();
        Func<Task> act = () => handler.Handle(new SaleCancelledEvent(sale), CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "ItemCancelledEventHandler manipula sem lançar exceção")]
    public async Task ItemCancelled_Handles()
    {
        var handler = new ItemCancelledEventHandler(Substitute.For<ILogger<ItemCancelledEventHandler>>());
        var sale = SaleTestData.GenerateSaleWithItems(1);
        var ev = new ItemCancelledEvent(sale, sale.Items.First());
        Func<Task> act = () => handler.Handle(ev, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
