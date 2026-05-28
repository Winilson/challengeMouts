using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.Mapping;

public class SaleProfilesTests
{
    private static IMapper MapperWith<TProfile>() where TProfile : Profile, new()
        => new MapperConfiguration(cfg => cfg.AddProfile(new TProfile())).CreateMapper();

    [Fact(DisplayName = "CreateSaleProfile: Venda mapeia para CreateSaleResult com itens")]
    public void CreateSaleProfile_Maps()
    {
        var mapper = MapperWith<CreateSaleProfile>();
        var sale = SaleTestData.GenerateSaleWithItems(2);

        var result = mapper.Map<CreateSaleResult>(sale);

        result.SaleNumber.Should().Be(sale.SaleNumber);
        result.Items.Should().HaveCount(2);
        result.TotalAmount.Should().Be(sale.TotalAmount);
    }

    [Fact(DisplayName = "GetSaleProfile: Venda mapeia para GetSaleResult com itens")]
    public void GetSaleProfile_Maps()
    {
        var mapper = MapperWith<GetSaleProfile>();
        var sale = SaleTestData.GenerateSaleWithItems(1);

        var result = mapper.Map<GetSaleResult>(sale);

        result.Id.Should().Be(sale.Id);
        result.Items.Should().HaveCount(1);
    }

    [Fact(DisplayName = "UpdateSaleProfile: Sale maps to UpdateSaleResult")]
    public void UpdateSaleProfile_Maps()
    {
        var mapper = MapperWith<UpdateSaleProfile>();
        var sale = SaleTestData.GenerateValidSale();

        var result = mapper.Map<UpdateSaleResult>(sale);

        result.Id.Should().Be(sale.Id);
        result.CustomerName.Should().Be(sale.CustomerName);
    }

    [Fact(DisplayName = "ListSalesProfile: Sale list maps to GetSaleResult list")]
    public void ListSalesProfile_Maps()
    {
        var mapper = MapperWith<ListSalesProfile>();
        var sales = new List<Sale>
        {
            SaleTestData.GenerateValidSale(),
            SaleTestData.GenerateValidSale()
        };

        var result = mapper.Map<List<GetSaleResult>>(sales);

        result.Should().HaveCount(2);
    }
}
