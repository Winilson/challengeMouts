using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesHandler : IRequestHandler<ListSalesQuery, ListSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public ListSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<ListSalesResult> Handle(ListSalesQuery request, CancellationToken cancellationToken)
    {
        // Sanitiza paginação (clamp entre 1 e 100)
        var page = request.Page < 1 ? 1 : request.Page;
        var size = request.Size < 1 ? 10 : Math.Min(request.Size, 100);

        // Monta a query — IQueryable não executa SQL ainda
        var query = _saleRepository.Query();
        query = ApplyFilters(query, request.Filters);
        query = ApplyOrdering(query, request.Order);

        // Conta total e pega a página (executa SQL aqui)
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return new ListSalesResult
        {
            Data = _mapper.Map<List<GetSaleResult>>(items),
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)size),
            TotalCount = totalCount
        };
    }

    private static IQueryable<Sale> ApplyFilters(IQueryable<Sale> query, IDictionary<string, string> filters)
    {
        foreach (var (rawKey, rawValue) in filters)
        {
            if (string.IsNullOrWhiteSpace(rawValue)) continue;
            var key = rawKey.Trim();
            var value = rawValue.Trim();

            // Range filters: _minDate, _maxDate
            if (key.StartsWith("_min", StringComparison.OrdinalIgnoreCase))
            { query = ApplyRange(query, key[4..], value, isMin: true); continue; }

            if (key.StartsWith("_max", StringComparison.OrdinalIgnoreCase))
            { query = ApplyRange(query, key[4..], value, isMin: false); continue; }

            // Equality / wildcard
            query = key.ToLowerInvariant() switch
            {
                "salenumber" => ApplyStringLike(query, value, nameof(Sale.SaleNumber)),
                "customername" => ApplyStringLike(query, value, nameof(Sale.CustomerName)),
                "branchname" => ApplyStringLike(query, value, nameof(Sale.BranchName)),
                "customerid" => Guid.TryParse(value, out var cid) ? query.Where(s => s.CustomerId == cid) : query,
                "branchid" => Guid.TryParse(value, out var bid) ? query.Where(s => s.BranchId == bid) : query,
                "iscancelled" => bool.TryParse(value, out var c) ? query.Where(s => s.IsCancelled == c) : query,
                _ => query
            };
        }
        return query;
    }

    private static IQueryable<Sale> ApplyStringLike(IQueryable<Sale> query, string value, string field)
    {
        // value pode ter wildcards: "*Joh*" / "Joh*" / "*hn"
        var startsWild = value.StartsWith('*');
        var endsWild = value.EndsWith('*');
        var pattern = value.Trim('*');
        var pgPattern = (startsWild, endsWild) switch
        {
            (true, true) => $"%{pattern}%",
            (false, true) => $"{pattern}%",
            (true, false) => $"%{pattern}",
            _ => pattern
        };

        return field switch
        {
            nameof(Sale.SaleNumber) => query.Where(s => EF.Functions.ILike(s.SaleNumber, pgPattern)),
            nameof(Sale.CustomerName) => query.Where(s => EF.Functions.ILike(s.CustomerName, pgPattern)),
            nameof(Sale.BranchName) => query.Where(s => EF.Functions.ILike(s.BranchName, pgPattern)),
            _ => query
        };
    }

    private static IQueryable<Sale> ApplyRange(IQueryable<Sale> query, string field, string value, bool isMin)
    {
        if (string.Equals(field, "Date", StringComparison.OrdinalIgnoreCase) &&
            DateTime.TryParse(value, out var date))
        {
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            return isMin ? query.Where(s => s.Date >= date) : query.Where(s => s.Date <= date);
        }
        return query;
    }

    private static IOrderedQueryable<Sale> ApplyOrdering(IQueryable<Sale> query, string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return query.OrderByDescending(s => s.Date); // default: mais recentes primeiro

        var clauses = order.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        IOrderedQueryable<Sale>? ordered = null;

        foreach (var clause in clauses)
        {
            var parts = clause.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = parts[0].ToLowerInvariant();
            var desc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            ordered = ordered == null ? FirstOrder(query, field, desc) : ThenOrder(ordered, field, desc);
        }
        return ordered ?? query.OrderByDescending(s => s.Date);
    }

    private static IOrderedQueryable<Sale> FirstOrder(IQueryable<Sale> q, string field, bool desc) => field switch
    {
        "salenumber" => desc ? q.OrderByDescending(s => s.SaleNumber) : q.OrderBy(s => s.SaleNumber),
        "date" => desc ? q.OrderByDescending(s => s.Date) : q.OrderBy(s => s.Date),
        "customername" => desc ? q.OrderByDescending(s => s.CustomerName) : q.OrderBy(s => s.CustomerName),
        "branchname" => desc ? q.OrderByDescending(s => s.BranchName) : q.OrderBy(s => s.BranchName),
        "createdat" => desc ? q.OrderByDescending(s => s.CreatedAt) : q.OrderBy(s => s.CreatedAt),
        _ => q.OrderByDescending(s => s.Date)
    };

    private static IOrderedQueryable<Sale> ThenOrder(IOrderedQueryable<Sale> q, string field, bool desc) => field switch
    {
        "salenumber" => desc ? q.ThenByDescending(s => s.SaleNumber) : q.ThenBy(s => s.SaleNumber),
        "date" => desc ? q.ThenByDescending(s => s.Date) : q.ThenBy(s => s.Date),
        "customername" => desc ? q.ThenByDescending(s => s.CustomerName) : q.ThenBy(s => s.CustomerName),
        "branchname" => desc ? q.ThenByDescending(s => s.BranchName) : q.ThenBy(s => s.BranchName),
        "createdat" => desc ? q.ThenByDescending(s => s.CreatedAt) : q.ThenBy(s => s.CreatedAt),
        _ => q
    };
}
