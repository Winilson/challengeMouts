using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(c => c.SaleNumber).NotEmpty().MaximumLength(50);
        RuleFor(c => c.CustomerId).NotEqual(Guid.Empty);
        RuleFor(c => c.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.BranchId).NotEqual(Guid.Empty);
        RuleFor(c => c.BranchName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Items).NotEmpty().WithMessage("A venda deve ter pelo menos um item.");

        // valida cada item da lista usando FluentValidation
        RuleForEach(c => c.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEqual(Guid.Empty);
            item.RuleFor(i => i.ProductName).NotEmpty().MaximumLength(200);

            // Capturada antes de chegar no construtor do SaleItem.
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0)
                .LessThanOrEqualTo(SaleItem.MaxQuantityPerProduct)
                .WithMessage($"Não é permitido vender mais de {SaleItem.MaxQuantityPerProduct} itens iguais.");

            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
