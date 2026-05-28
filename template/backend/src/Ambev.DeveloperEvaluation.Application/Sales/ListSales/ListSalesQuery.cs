// ═══════════════════════════════════════════════════════════════════════════
// ARQUIVO: ListSalesQuery.cs
// VS:    Solution > Core > Aplication > Sales > ListSales > ListSalesQuery.cs
// DISCO: backend/src/Ambev.DeveloperEvaluation.Application/Sales/ListSales/ListSalesQuery.cs
// ───────────────────────────────────────────────────────────────────────────
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesQuery : IRequest<ListSalesResult>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Order { get; set; }
    public IDictionary<string, string> Filters { get; set; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

public class ListSalesResult
{
    public List<GetSaleResult> Data { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}
