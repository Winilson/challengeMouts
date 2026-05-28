using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

public class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(r => r.SaleNumber).NotEmpty().MaximumLength(50);
        RuleFor(r => r.CustomerId).NotEqual(Guid.Empty);
        RuleFor(r => r.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(r => r.BranchId).NotEqual(Guid.Empty);
        RuleFor(r => r.BranchName).NotEmpty().MaximumLength(200);
        RuleFor(r => r.Items).NotEmpty();

        RuleForEach(r => r.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEqual(Guid.Empty);
            item.RuleFor(i => i.ProductName).NotEmpty().MaximumLength(200);
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0)
                .LessThanOrEqualTo(SaleItem.MaxQuantityPerProduct)
               .WithMessage($"Não é permitido vender mais de {SaleItem.MaxQuantityPerProduct} itens iguais.");
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
