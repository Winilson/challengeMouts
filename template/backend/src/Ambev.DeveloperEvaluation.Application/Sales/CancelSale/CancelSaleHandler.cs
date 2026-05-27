using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, bool>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMediator _mediator;

    public CancelSaleHandler(ISaleRepository saleRepository, IMediator mediator)
    {
        _saleRepository = saleRepository;
        _mediator = mediator;
    }

    public async Task<bool> Handle(CancelSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new CancelSaleValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Venda com ID {command.Id} não encontrada.");

        sale.Cancel(); // dispara SaleCancelledEvent + cascateia itens
        await _saleRepository.UpdateAsync(sale, cancellationToken);

        var events = sale.DomainEvents.ToList();
        sale.ClearDomainEvents();
        foreach (var e in events) await _mediator.Publish(e, cancellationToken);

        return true;
    }
}
