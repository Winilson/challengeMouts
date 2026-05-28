using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.UpdateSale;

public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _repo = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests() => _handler = new UpdateSaleHandler(_repo, _mapper, _mediator);

    [Fact(DisplayName = "comando válido, quando manipular, então atualiza e publica o evento")]
    public async Task Handle_Valid_UpdatesAndPublishes()
    {
        var sale = SaleTestData.GenerateValidSale();
        _repo.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<UpdateSaleResult>(sale).Returns(new UpdateSaleResult { Id = sale.Id });

        var command = new UpdateSaleCommand { Id = sale.Id, CustomerName = "Novo", BranchName = "Filial X" };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Id.Should().Be(sale.Id);
        sale.CustomerName.Should().Be("Novo");
        await _repo.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
        await _mediator.Received(1).Publish(Arg.Any<SaleModifiedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Dada uma venda inexistente, lança KeyNotFoundException")]
    public async Task Handle_NotFound_Throws()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);
        var command = new UpdateSaleCommand { Id = Guid.NewGuid(), CustomerName = "X", BranchName = "Y" };
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Dado um nome de cliente vazio,lança ValidationException")]
    public async Task Handle_EmptyCustomerName_ThrowsValidation()
    {
        var command = new UpdateSaleCommand { Id = Guid.NewGuid(), CustomerName = "", BranchName = "Y" };
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }
}
