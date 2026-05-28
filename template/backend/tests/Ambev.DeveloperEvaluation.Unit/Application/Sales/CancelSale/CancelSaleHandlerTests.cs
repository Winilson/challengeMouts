using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using FluentValidation;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.CancelSale;

public class CancelSaleHandlerTests
{
    private readonly ISaleRepository _repo = Substitute.For<ISaleRepository>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly CancelSaleHandler _handler;

    public CancelSaleHandlerTests() => _handler = new CancelSaleHandler(_repo, _mediator);

    [Fact(DisplayName = "venda existente, quando cancelar, então retorna true e publica o evento")]
    public async Task Handle_Existing_CancelsAndPublishes()
    {
        var sale = SaleTestData.GenerateSaleWithItems(2);
        _repo.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var result = await _handler.Handle(new CancelSaleCommand(sale.Id), CancellationToken.None);

        result.Should().BeTrue();
        sale.IsCancelled.Should().BeTrue();
        await _mediator.Received(1).Publish(Arg.Any<SaleCancelledEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "venda inexistente, quando cancelar, então lança KeyNotFoundException")]
    public async Task Handle_NotFound_Throws()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);
        Func<Task> act = () => _handler.Handle(new CancelSaleCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "ID vazio, quando cancelar, então lança ValidationException")]
    public async Task Handle_EmptyId_ThrowsValidation()
    {
        Func<Task> act = () => _handler.Handle(new CancelSaleCommand(Guid.Empty), CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }
}
