using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.Validators;

public class SaleValidatorsTests
{
    [Fact(DisplayName = "CreateSaleCommandValidator: comando válido passa na validação")]
    public void Create_Valid_Passes()
    {
        var cmd = new CreateSaleCommand
        {
            SaleNumber = "S-1",
            CustomerId = Guid.NewGuid(),
            CustomerName = "C",
            BranchId = Guid.NewGuid(),
            BranchName = "B",
            Items = new() { new CreateSaleItemDto { ProductId = Guid.NewGuid(), ProductName = "P", Quantity = 5, UnitPrice = 10 } }
        };
        new CreateSaleCommandValidator().Validate(cmd).IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "CreateSaleCommandValidator: quantidade acima de 20 falha na validação")]
    public void Create_QuantityOver20_Fails()
    {
        var cmd = new CreateSaleCommand
        {
            SaleNumber = "S-1",
            CustomerId = Guid.NewGuid(),
            CustomerName = "C",
            BranchId = Guid.NewGuid(),
            BranchName = "B",
            Items = new() { new CreateSaleItemDto { ProductId = Guid.NewGuid(), ProductName = "P", Quantity = 21, UnitPrice = 10 } }
        };
        new CreateSaleCommandValidator().Validate(cmd).IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "CreateSaleCommandValidator: itens vazios falham na validação")]
    public void Create_NoItems_Fails()
    {
        var cmd = new CreateSaleCommand
        {
            SaleNumber = "S-1",
            CustomerId = Guid.NewGuid(),
            CustomerName = "C",
            BranchId = Guid.NewGuid(),
            BranchName = "B",
            Items = new()
        };
        new CreateSaleCommandValidator().Validate(cmd).IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "GetSaleValidator: ID vazio falha na validação")]
    public void Get_EmptyId_Fails()
    {
        new GetSaleValidator().Validate(new GetSaleQuery(Guid.Empty)).IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "GetSaleValidator: ID válido passa na validação")]
    public void Get_ValidId_Passes()
    {
        new GetSaleValidator().Validate(new GetSaleQuery(Guid.NewGuid())).IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "UpdateSaleValidator: nome vazio falha na validação")]
    public void Update_EmptyName_Fails()
    {
        var cmd = new UpdateSaleCommand { Id = Guid.NewGuid(), CustomerName = "", BranchName = "B" };
        new UpdateSaleValidator().Validate(cmd).IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "CancelSaleValidator: ID vazio falha na validação")]
    public void Cancel_EmptyId_Fails()
    {
        new CancelSaleValidator().Validate(new CancelSaleCommand(Guid.Empty)).IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "CancelSaleItemValidator: IDs vazios falham na validação")]
    public void CancelItem_EmptyIds_Fails()
    {
        var cmd = new CancelSaleItemCommand { SaleId = Guid.Empty, ItemId = Guid.Empty };
        new CancelSaleItemValidator().Validate(cmd).IsValid.Should().BeFalse();
    }
}
