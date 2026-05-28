using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

[ApiController]
[Route("api/[controller]")]
public class SalesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public SalesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

   
    // POST /api/Sales — criar venda
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateSale(
        [FromBody] CreateSaleRequest request,
        CancellationToken cancellationToken)
    {
        //valida o shape do request (FluentValidation)
        var validator = new CreateSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        //mapeia Request → Command via AutoMapper
        var command = _mapper.Map<CreateSaleCommand>(request);

        // envia para o MediatR 
        var response = await _mediator.Send(command, cancellationToken);

        //devolve 201 Created com o resultado mapeado
        return Created(string.Empty, new ApiResponseWithData<CreateSaleResponse>
        {
            Success = true,
            Message = "Venda criada com sucesso",
            Data = _mapper.Map<CreateSaleResponse>(response)
        });
    }

    
    // GET/api/Sales/{id} — buscar venda por id
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetSaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSale(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var request = new GetSaleRequest { Id = id };
        var validator = new GetSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var query = new GetSaleQuery(request.Id);
        var response = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponseWithData<GetSaleResponse>
        {
            Success = true,
            Message = "Venda recuperada com sucesso",
            Data = _mapper.Map<GetSaleResponse>(response)
        });
    }

   
    // GET /api/Sales — listar com paginação/filtros/ordenação
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<GetSaleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListSales(
        [FromQuery(Name = "_page")] int page = 1,
        [FromQuery(Name = "_size")] int size = 10,
        [FromQuery(Name = "_order")] string? order = null,
        CancellationToken cancellationToken = default)
    {
        var reserved = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "_page", "_size", "_order" };
        var filters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in HttpContext.Request.Query)
        {
            if (reserved.Contains(kvp.Key)) continue;
            filters[kvp.Key] = kvp.Value.ToString();
        }

        var query = new ListSalesQuery
        {
            Page = page,
            Size = size,
            Order = order,
            Filters = filters
        };
        var result = await _mediator.Send(query, cancellationToken);

        var data = _mapper.Map<List<GetSaleResponse>>(result.Data);
        return Ok(new PaginatedResponse<GetSaleResponse>
        {
            Success = true,
            Message = "Vendas recuperadas com sucesso",
            Data = data,
            CurrentPage = result.CurrentPage,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount
        });
    }

   
    // PUT /api/Sales/{id} — atualizar cliente/filial
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<UpdateSaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSale(
        [FromRoute] Guid id,
        [FromBody] UpdateSaleRequest request,
        CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<UpdateSaleCommand>(request);
        command.Id = id;
        var response = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponseWithData<UpdateSaleResponse>
        {
            Success = true,
            Message = "Venda atualizada com sucesso",
            Data = _mapper.Map<UpdateSaleResponse>(response)
        });
    }

    
    // DELETE /api/Sales/{id} — cancelar venda inteira (cascateia itens)
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSale(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelSaleCommand(id), cancellationToken);

        return Ok(new ApiResponse
        {
            Success = true,
            Message = "Venda cancelada com sucesso."
        });
    }

    // DELETE /api/Sales/{saleId}/items/{itemId} — cancelar UM item
    [HttpDelete("{saleId}/items/{itemId}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSaleItem(
        [FromRoute] Guid saleId,
        [FromRoute] Guid itemId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelSaleItemCommand { SaleId = saleId, ItemId = itemId }, cancellationToken);

        return Ok(new ApiResponse
        {
            Success = true,
            Message = "Item da venda cancelada com sucesso."
        });
    }
}
