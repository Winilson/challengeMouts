using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.CreateSale;

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _repo = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _handler = new CreateSaleHandler(_repo, _mapper, _mediator);
    }

    [Fact(DisplayName = "cria a venda e publica o evento")]
    public async Task Handle_ValidCommand_CreatesAndPublishes()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand(2);
        _repo.ExistsBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>()).Returns(false);
        _repo.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Sale>());
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>())
            .Returns(new CreateSaleResult { SaleNumber = command.SaleNumber });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.SaleNumber.Should().Be(command.SaleNumber);
        await _repo.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _mediator.Received(1).Publish(Arg.Any<SaleCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "sem itens, então lança ValidationException")]
    public async Task Handle_NoItems_ThrowsValidation()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand(0);
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
        await _repo.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given duplicate sale number When handling Then InvalidOperationException")]
    public async Task Handle_DuplicateSaleNumber_ThrowsConflict()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        _repo.ExistsBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>()).Returns(true);

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact(DisplayName = "item com quantidade acima de 20, então lança ValidationException")]
    public async Task Handle_QuantityOver20_ThrowsValidation()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand(1);
        command.Items[0].Quantity = 21;

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }
}
