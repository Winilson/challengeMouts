using FluentValidation;
namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

public class UpdateSaleRequestValidator : AbstractValidator<UpdateSaleRequest>
{
    public UpdateSaleRequestValidator()
    {
        RuleFor(r => r.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(r => r.BranchName).NotEmpty().MaximumLength(200);
    }
}
