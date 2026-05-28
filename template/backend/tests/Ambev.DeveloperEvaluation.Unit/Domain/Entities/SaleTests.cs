using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Fact(DisplayName = "Dados válidos, quando criar, então dispara SaleCreatedEvent")]
    public void Constructor_ValidData_RaisesSaleCreatedEvent()
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<SaleCreatedEvent>();
    }

    [Fact(DisplayName = "Dados válidos, quando criar, então as propriedades são preenchidas e a venda não fica cancelada")]
    public void Constructor_ValidData_SetsProperties()
    {
        var sale = new Sale("SALE-1", Guid.NewGuid(), "Cliente", Guid.NewGuid(), "Filial");
        sale.SaleNumber.Should().Be("SALE-1");
        sale.CustomerName.Should().Be("Cliente");
        sale.BranchName.Should().Be("Filial");
        sale.IsCancelled.Should().BeFalse();
        sale.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory(DisplayName = "Dado um campo obrigatório inválido, quando criar, então lança DomainException")]
    [InlineData("", true, true, true, true)]    // saleNumber vazio
    [InlineData("S1", false, true, true, true)] // customerId vazio
    [InlineData("S1", true, false, true, true)] // customerName vazio
    [InlineData("S1", true, true, false, true)] // branchId vazio
    [InlineData("S1", true, true, true, false)] // branchName vazio
    public void Constructor_InvalidField_Throws(string saleNumber, bool custId, bool custName, bool branchId, bool branchName)
    {
        Action act = () => new Sale(
            saleNumber,
            custId ? Guid.NewGuid() : Guid.Empty,
            custName ? "Cliente" : "",
            branchId ? Guid.NewGuid() : Guid.Empty,
            branchName ? "Filial" : "");

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Dado um item válido, quando adicionar item, então o item é adicionado")]
    public void AddItem_Valid_AddsItem()
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.AddItem(Guid.NewGuid(), "Cerveja", 3, 5m);
        sale.Items.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Dado um produto ativo duplicado, quando adicionar item, então lança DomainException")]
    public void AddItem_DuplicateProduct_Throws()
    {
        var sale = SaleTestData.GenerateValidSale();
        var productId = Guid.NewGuid();
        sale.AddItem(productId, "Cerveja", 3, 5m);

        Action act = () => sale.AddItem(productId, "Cerveja", 2, 5m);
        act.Should().Throw<DomainException>().WithMessage("*já está nesta venda*");
    }

    [Fact(DisplayName = "Dada uma venda cancelada, quando adicionar item, então lança DomainException")]
    public void AddItem_CancelledSale_Throws()
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.Cancel();

        Action act = () => sale.AddItem(Guid.NewGuid(), "P", 1, 1m);
        act.Should().Throw<DomainException>();
    }

  
    [Fact(DisplayName = "Dados os itens, quando somar, então TotalAmount está correto")]
    public void TotalAmount_WithItems_SumsCorrectly()
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.AddItem(Guid.NewGuid(), "Item1", 2, 10m); // 20 (sem desconto)
        sale.AddItem(Guid.NewGuid(), "Item2", 5, 10m); // 45 (10% off)
        sale.TotalAmount.Should().Be(65m);
    }

    [Fact(DisplayName = "Dado um item cancelado, quando somar, então ele é excluído do TotalAmount")]
    public void TotalAmount_ExcludesCancelledItems()
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.AddItem(Guid.NewGuid(), "Item1", 2, 10m); // 20
        sale.AddItem(Guid.NewGuid(), "Item2", 5, 10m); // 45
        foreach (var item in sale.Items) item.Id = Guid.NewGuid();

        sale.CancelItem(sale.Items.First().Id);

        sale.TotalAmount.Should().Be(45m); // só o segundo conta
    }

    [Fact(DisplayName = "Dada uma venda com itens, quando cancelar, então todos os itens são cancelados")]
    public void Cancel_WithItems_CascadesToItems()
    {
        var sale = SaleTestData.GenerateSaleWithItems(3);
        sale.Cancel();

        sale.IsCancelled.Should().BeTrue();
        sale.Items.Should().AllSatisfy(i => i.IsCancelled.Should().BeTrue());
    }

    [Fact(DisplayName = "Dada uma venda, quando cancelar, então dispara SaleCancelledEvent")]
    public void Cancel_RaisesSaleCancelledEvent()
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.ClearDomainEvents();
        sale.Cancel();
        sale.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<SaleCancelledEvent>();
    }

    [Fact(DisplayName = "Dada uma venda cancelada, quando cancelar novamente, então lança DomainException")]
    public void Cancel_AlreadyCancelled_Throws()
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.Cancel();
        Action act = () => sale.Cancel();
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Dada uma venda, quando cancelar um item, então a venda permanece ativa")]
    public void CancelItem_OneItem_SaleStaysActive()
    {
        var sale = SaleTestData.GenerateSaleWithItems(2);
        var firstId = sale.Items.First().Id;

        sale.CancelItem(firstId);

        sale.IsCancelled.Should().BeFalse();
        sale.Items.First(i => i.Id == firstId).IsCancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "Dada uma venda, quando cancelar item, então dispara ItemCancelledEvent")]
    public void CancelItem_RaisesItemCancelledEvent()
    {
        var sale = SaleTestData.GenerateSaleWithItems(1);
        sale.ClearDomainEvents();
        sale.CancelItem(sale.Items.First().Id);
        sale.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<ItemCancelledEvent>();
    }

    [Fact(DisplayName = "Dado um item inexistente, quando cancelar, então lança DomainException")]
    public void CancelItem_NotFound_Throws()
    {
        var sale = SaleTestData.GenerateSaleWithItems(1);
        Action act = () => sale.CancelItem(Guid.NewGuid());
        act.Should().Throw<DomainException>().WithMessage("*não foi encontrado*");
    }

    [Fact(DisplayName = "Dada uma venda cancelada, quando cancelar item, então lança DomainException")]
    public void CancelItem_CancelledSale_Throws()
    {
        var sale = SaleTestData.GenerateSaleWithItems(1);
        var itemId = sale.Items.First().Id;
        sale.Cancel();

        Action act = () => sale.CancelItem(itemId);
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Dada uma venda, quando atualizar as informações básicas, então os valores mudam e o evento é disparado")]
    public void UpdateBasicInfo_Valid_UpdatesAndRaisesEvent()
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.ClearDomainEvents();

        sale.UpdateBasicInfo("Novo Cliente", "Nova Filial");

        sale.CustomerName.Should().Be("Novo Cliente");
        sale.BranchName.Should().Be("Nova Filial");
        sale.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<SaleModifiedEvent>();
    }

    [Fact(DisplayName = "Dada uma venda cancelada, quando atualizar, então lança DomainException")]
    public void UpdateBasicInfo_CancelledSale_Throws()
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.Cancel();
        Action act = () => sale.UpdateBasicInfo("X", "Y");
        act.Should().Throw<DomainException>();
    }

    [Theory(DisplayName = "Dado um campo vazio, quando atualizar, então lança DomainException")]
    [InlineData("", "Filial")]
    [InlineData("Cliente", "")]
    public void UpdateBasicInfo_EmptyField_Throws(string customerName, string branchName)
    {
        var sale = SaleTestData.GenerateValidSale();
        Action act = () => sale.UpdateBasicInfo(customerName, branchName);
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Dada uma venda válida, quando validar, então IsValid é true")]
    public void Validate_ValidSale_ReturnsValid()
    {
        var sale = SaleTestData.GenerateSaleWithItems(1);
        var result = sale.Validate();
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Dados os eventos acumulados, quando limpar, então DomainEvents fica vazio")]
    public void ClearDomainEvents_RemovesAll()
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.DomainEvents.Should().NotBeEmpty();
        sale.ClearDomainEvents();
        sale.DomainEvents.Should().BeEmpty();
    }
}
