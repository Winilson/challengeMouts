using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using Ambev.DeveloperEvaluation.Unit.ORM;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.ListSales;

public class ListSalesHandlerTests
{
    private static IMapper BuildMapper()
    {
        // Mapper mockado: devolve um GetSaleResult por venda (lista não-vazia).
        var mapper = Substitute.For<IMapper>();
        mapper.Map<List<GetSaleResult>>(Arg.Any<object>())
            .Returns(ci =>
            {
                var src = ci.Arg<object>() as IEnumerable<Sale>;
                return src == null
                    ? new List<GetSaleResult>()
                    : src.Select(s => new GetSaleResult { Id = s.Id, SaleNumber = s.SaleNumber }).ToList();
            });
        return mapper;
    }

    [Fact(DisplayName = "Given 3 sales When listing page 1 size 2 Then returns 2 and total 3")]
    public async Task Handle_Pagination_Works()
    {
        using var ctx = InMemoryContextFactory.Create();
        var repo = new SaleRepository(ctx);
        for (int i = 0; i < 3; i++) await repo.CreateAsync(SaleTestData.GenerateValidSale());

        var handler = new ListSalesHandler(repo, BuildMapper());
        var result = await handler.Handle(new ListSalesQuery { Page = 1, Size = 2 }, CancellationToken.None);

        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(2);
        result.CurrentPage.Should().Be(1);
        result.Data.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Given no sales When listing Then returns empty")]
    public async Task Handle_Empty_ReturnsEmpty()
    {
        using var ctx = InMemoryContextFactory.Create();
        var repo = new SaleRepository(ctx);
        var handler = new ListSalesHandler(repo, BuildMapper());

        var result = await handler.Handle(new ListSalesQuery(), CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    [Fact(DisplayName = "Given order by saleNumber When listing Then no exception")]
    public async Task Handle_WithOrdering_Works()
    {
        using var ctx = InMemoryContextFactory.Create();
        var repo = new SaleRepository(ctx);
        await repo.CreateAsync(SaleTestData.GenerateValidSale());

        var handler = new ListSalesHandler(repo, BuildMapper());
        var result = await handler.Handle(
            new ListSalesQuery { Order = "saleNumber desc" }, CancellationToken.None);

        result.TotalCount.Should().Be(1);
    }
}
