using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IMediator mediator)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {command.Id} not found.");

        // UpdateBasicInfo aplica regras (não pode atualizar cancelada)
        // e dispara SaleModifiedEvent internamente.
        sale.UpdateBasicInfo(command.CustomerName, command.BranchName);

        await _saleRepository.UpdateAsync(sale, cancellationToken);

        // Despacha eventos acumulados na entidade
        var events = sale.DomainEvents.ToList();
        sale.ClearDomainEvents();
        foreach (var e in events) await _mediator.Publish(e, cancellationToken);

        return _mapper.Map<UpdateSaleResult>(sale);
    }
}
