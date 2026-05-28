using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using FluentValidation;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _repo = Substitute.For<ISaleRepository>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests() => _handler = new CancelSaleItemHandler(_repo, _mediator);

    [Fact(DisplayName = "item existente, quando cancelar, então retorna true e publica o evento")]
    public async Task Handle_Existing_CancelsItemAndPublishes()
    {
        var sale = SaleTestData.GenerateSaleWithItems(2);
        var itemId = sale.Items.First().Id;
        _repo.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var command = new CancelSaleItemCommand { SaleId = sale.Id, ItemId = itemId };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        sale.Items.First(i => i.Id == itemId).IsCancelled.Should().BeTrue();
        sale.IsCancelled.Should().BeFalse();
        await _mediator.Received(1).Publish(Arg.Any<ItemCancelledEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "venda inexistente, quando cancelar item, então lança KeyNotFoundException")]
    public async Task Handle_SaleNotFound_Throws()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);
        var command = new CancelSaleItemCommand { SaleId = Guid.NewGuid(), ItemId = Guid.NewGuid() };
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = " IDs vazios, quando cancelar item, então lança ValidationException")]
    public async Task Handle_EmptyIds_ThrowsValidation()
    {
        var command = new CancelSaleItemCommand { SaleId = Guid.Empty, ItemId = Guid.Empty };
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }
}
