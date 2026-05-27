using FluentValidation;
namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public class GetSaleValidator : AbstractValidator<GetSaleQuery>
{
    public GetSaleValidator()
    {
        RuleFor(q => q.Id).NotEqual(Guid.Empty).WithMessage("O ID da venda é obrigatório.");
    }
}
