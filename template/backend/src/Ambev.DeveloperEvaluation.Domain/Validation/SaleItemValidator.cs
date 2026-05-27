using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

public class SaleItemValidator : AbstractValidator<SaleItem>
{
    public SaleItemValidator()
    {
        RuleFor(i => i.ProductId)
       .NotEqual(Guid.Empty).WithMessage("O ID do produto é obrigatório.");

        RuleFor(i => i.ProductName)
            .NotEmpty().WithMessage("O nome do produto é obrigatório.")
            .MaximumLength(200);

        RuleFor(i => i.Quantity)
            .GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.")
            .LessThanOrEqualTo(SaleItem.MaxQuantityPerProduct)
            .WithMessage($"Não é permitido vender mais de {SaleItem.MaxQuantityPerProduct} itens iguais.");

        RuleFor(i => i.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("O preço unitário não pode ser negativo.");
    }
}
