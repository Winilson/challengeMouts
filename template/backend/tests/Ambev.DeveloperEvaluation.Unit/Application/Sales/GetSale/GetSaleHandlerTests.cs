using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.GetSale;

public class GetSaleHandlerTests
{
    private readonly ISaleRepository _repo = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly GetSaleHandler _handler;

    public GetSaleHandlerTests() => _handler = new GetSaleHandler(_repo, _mapper);

    [Fact(DisplayName = "venda existente,retorna o resultado")]
    public async Task Handle_Existing_ReturnsResult()
    {
        var sale = SaleTestData.GenerateValidSale();
        _repo.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(new GetSaleResult { Id = sale.Id });

        var result = await _handler.Handle(new GetSaleQuery(sale.Id), CancellationToken.None);

        result.Id.Should().Be(sale.Id);
    }

    [Fact(DisplayName = " venda inexistente, lança KeyNotFoundException")]
    public async Task Handle_NotFound_Throws()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);
        Func<Task> act = () => _handler.Handle(new GetSaleQuery(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "venda inexistente,então lança KeyNotFoundException")]
    public async Task Handle_EmptyId_ThrowsValidation()
    {
        Func<Task> act = () => _handler.Handle(new GetSaleQuery(Guid.Empty), CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }
}
