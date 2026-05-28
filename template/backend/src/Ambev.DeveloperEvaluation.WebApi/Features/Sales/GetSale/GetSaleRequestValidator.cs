using FluentValidation;
namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;

public class GetSaleRequestValidator : AbstractValidator<GetSaleRequest>
{
    public GetSaleRequestValidator()
    {
        RuleFor(r => r.Id).NotEqual(Guid.Empty);
    }
}
