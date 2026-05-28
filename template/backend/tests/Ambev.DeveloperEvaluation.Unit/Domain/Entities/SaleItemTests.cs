using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleItemTests
{
    [Theory(DisplayName = "Dada uma quantidade abaixo de 4, quando calcular a taxa, então não aplica desconto")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CalculateDiscountRate_Below4_ReturnsZero(int quantity)
    {
        SaleItem.CalculateDiscountRate(quantity).Should().Be(0m);
    }

    [Theory(DisplayName = "Dada uma quantidade de 4 a 9, quando calcular a taxa, então aplica 10%")]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(9)]
    public void CalculateDiscountRate_4To9_Returns10Percent(int quantity)
    {
        SaleItem.CalculateDiscountRate(quantity).Should().Be(0.10m);
    }

    [Theory(DisplayName = "Dada uma quantidade de 10 a 20, quando calcular a taxa, então aplica 20%")]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void CalculateDiscountRate_10To20_Returns20Percent(int quantity)
    {
        SaleItem.CalculateDiscountRate(quantity).Should().Be(0.20m);
    }

    [Fact(DisplayName = "Dada uma quantidade acima de 20, quando calcular a taxa, então retorna zero por segurança")]
    public void CalculateDiscountRate_Above20_ReturnsZero()
    {
        SaleItem.CalculateDiscountRate(21).Should().Be(0m);
    }

    [Fact(DisplayName = "Dada uma quantidade acima de 20, quando criar, então lança DomainException")]
    public void Constructor_QuantityOver20_Throws()
    {
        Action act = () => new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "Cerveja", 21, 5m);
        act.Should().Throw<DomainException>().WithMessage("*20*");
    }

    [Theory(DisplayName = "Dada uma quantidade não positiva, quando criar, então lança DomainException")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_NonPositiveQuantity_Throws(int quantity)
    {
        Action act = () => new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "P", quantity, 5m);
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Dado um nome de produto vazio, quando criar, então lança DomainException")]
    public void Constructor_EmptyProductName_Throws()
    {
        Action act = () => new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "", 5, 10m);
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Dado um preço unitário negativo, quando criar, então lança DomainException")]
    public void Constructor_NegativeUnitPrice_Throws()
    {
        Action act = () => new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "P", 5, -1m);
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Dada uma quantidade de 3 itens a 10, quando criar, então não aplica desconto e o total é 30")]
    public void Constructor_3ItemsAt10_NoDiscount()
    {
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "P", 3, 10m);
        item.Discount.Should().Be(0m);
        item.TotalAmount.Should().Be(30m);
    }

    [Fact(DisplayName = "Dada uma quantidade de 5 itens a 10, quando criar, então aplica desconto de 5 e o total é 45")]
    public void Constructor_5ItemsAt10_Discount10Percent()
    {
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "P", 5, 10m);
        item.Discount.Should().Be(5m);
        item.TotalAmount.Should().Be(45m);
    }

    [Fact(DisplayName = "Dada uma quantidade de 10 itens a 10, quando criar, então aplica desconto de 20 e o total é 80")]
    public void Constructor_10ItemsAt10_Discount20Percent()
    {
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "P", 10, 10m);
        item.Discount.Should().Be(20m);
        item.TotalAmount.Should().Be(80m);
    }

    [Fact(DisplayName = "Dados válidos, quando criar, então as propriedades são preenchidas")]
    public void Constructor_ValidData_SetsProperties()
    {
        var saleId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var item = new SaleItem(saleId, productId, "Brahma", 4, 4.50m);

        item.SaleId.Should().Be(saleId);
        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be("Brahma");
        item.Quantity.Should().Be(4);
        item.UnitPrice.Should().Be(4.50m);
        item.IsCancelled.Should().BeFalse();
    }

    [Fact(DisplayName = "Dado um item ativo, quando cancelar, então IsCancelled fica true")]
    public void Cancel_ActiveItem_SetsCancelled()
    {
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "P", 5, 10m);
        item.Cancel();
        item.IsCancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "Dado um item cancelado, quando cancelar novamente, então lança DomainException")]
    public void Cancel_AlreadyCancelled_Throws()
    {
        var item = new SaleItem(Guid.NewGuid(), Guid.NewGuid(), "P", 5, 10m);
        item.Cancel();
        Action act = () => item.Cancel();
        act.Should().Throw<DomainException>();
    }
}
