using MediatR;
namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public class GetSaleQuery : IRequest<GetSaleResult>
{
    public Guid Id { get; set; }
    public GetSaleQuery(Guid id) => Id = id;
    public GetSaleQuery() { }
}
