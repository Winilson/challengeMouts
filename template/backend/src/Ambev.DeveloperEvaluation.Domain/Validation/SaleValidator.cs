using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

public class SaleValidator : AbstractValidator<Sale>
{
    public SaleValidator()
    {
        RuleFor(s => s.SaleNumber)
            .NotEmpty().WithMessage("O número da venda é obrigatório.")
            .MaximumLength(50);

        RuleFor(s => s.CustomerId)
            .NotEqual(Guid.Empty).WithMessage("O ID do cliente é obrigatório.");

        RuleFor(s => s.CustomerName)
            .NotEmpty().WithMessage("O nome do cliente é obrigatório.")
            .MaximumLength(200);

        RuleFor(s => s.BranchId)
            .NotEqual(Guid.Empty).WithMessage("O ID da filial é obrigatório.");

        RuleFor(s => s.BranchName)
            .NotEmpty().WithMessage("O nome da filial é obrigatório.")
            .MaximumLength(200);

        RuleForEach(s => s.Items).SetValidator(new SaleItemValidator());
    }
}
