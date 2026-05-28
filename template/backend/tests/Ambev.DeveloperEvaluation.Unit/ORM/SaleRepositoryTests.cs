using Ambev.DeveloperEvaluation.ORM.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.ORM;

public class SaleRepositoryTests
{
    [Fact(DisplayName = "CreateAsync persiste a venda com itens")]
    public async Task CreateAsync_PersistsSale()
    {
        using var ctx = InMemoryContextFactory.Create();
        var repo = new SaleRepository(ctx);
        var sale = SaleTestData.GenerateSaleWithItems(2);

        var created = await repo.CreateAsync(sale);

        created.Id.Should().Be(sale.Id);
        var fromDb = await repo.GetByIdAsync(sale.Id);
        fromDb.Should().NotBeNull();
        fromDb!.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "GetByIdAsync retorna null quando não encontra")]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        using var ctx = InMemoryContextFactory.Create();
        var repo = new SaleRepository(ctx);
        (await repo.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact(DisplayName = "GetBySaleNumberAsync retorna a venda")]
    public async Task GetBySaleNumberAsync_ReturnsSale()
    {
        using var ctx = InMemoryContextFactory.Create();
        var repo = new SaleRepository(ctx);
        var sale = SaleTestData.GenerateValidSale();
        await repo.CreateAsync(sale);

        var found = await repo.GetBySaleNumberAsync(sale.SaleNumber);
        found.Should().NotBeNull();
        found!.SaleNumber.Should().Be(sale.SaleNumber);
    }

    [Fact(DisplayName = "ExistsBySaleNumberAsync retorna true quando existe")]
    public async Task ExistsBySaleNumberAsync_True()
    {
        using var ctx = InMemoryContextFactory.Create();
        var repo = new SaleRepository(ctx);
        var sale = SaleTestData.GenerateValidSale();
        await repo.CreateAsync(sale);

        (await repo.ExistsBySaleNumberAsync(sale.SaleNumber)).Should().BeTrue();
        (await repo.ExistsBySaleNumberAsync("INEXISTENTE-999")).Should().BeFalse();
    }

    [Fact(DisplayName = "UpdateAsync persiste as alterações")]
    public async Task UpdateAsync_PersistsChanges()
    {
        using var ctx = InMemoryContextFactory.Create();
        var repo = new SaleRepository(ctx);
        var sale = SaleTestData.GenerateValidSale();
        await repo.CreateAsync(sale);

        sale.UpdateBasicInfo("Cliente Alterado", "Filial Alterada");
        await repo.UpdateAsync(sale);

        var fromDb = await repo.GetByIdAsync(sale.Id);
        fromDb!.CustomerName.Should().Be("Cliente Alterado");
        fromDb.BranchName.Should().Be("Filial Alterada");
    }

    [Fact(DisplayName = "Query retorna IQueryable das vendas persistidas")]
    public async Task Query_ReturnsSales()
    {
        using var ctx = InMemoryContextFactory.Create();
        var repo = new SaleRepository(ctx);
        await repo.CreateAsync(SaleTestData.GenerateValidSale());
        await repo.CreateAsync(SaleTestData.GenerateValidSale());

        repo.Query().Should().HaveCount(2);
    }
}
