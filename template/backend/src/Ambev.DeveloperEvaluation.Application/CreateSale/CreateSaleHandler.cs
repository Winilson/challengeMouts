using Ambev.DeveloperEvaluation.Application.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IMediator mediator)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
      //Validação Manual
        var validator = new CreateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // SaleNumber deve ser único; validamos aqui porque o FluentValidation não acessa o repositório.
        var exists = await _saleRepository.ExistsBySaleNumberAsync(
            command.SaleNumber, cancellationToken);
        if (exists)
            throw new InvalidOperationException(
                $"Já existe uma venda com o número '{command.SaleNumber}'.");

        // Instancia o Sale, aplicando invariantes e disparando SaleCreatedEvent.
        var sale = new Sale(
            command.SaleNumber,
            command.CustomerId,
            command.CustomerName,
            command.BranchId,
            command.BranchName);

        // Adiciona os itens, aplicando regras de negócio e calculando descontos automaticamente.
        foreach (var itemDto in command.Items)
        {
            sale.AddItem(itemDto.ProductId, itemDto.ProductName, itemDto.Quantity, itemDto.UnitPrice);
        }

        // Persiste a venda e seus itens em uma única transação pelo EF Core.
        var created = await _saleRepository.CreateAsync(sale, cancellationToken);

        // Publica os eventos de domínio somente após a venda ser salva com sucesso.
        await DispatchEventsAsync(created, cancellationToken);

        // mapear para result
        return _mapper.Map<CreateSaleResult>(created);
    }

    // Helper explícito para despachar eventos de domínio após a persistência
    private async Task DispatchEventsAsync(Sale sale, CancellationToken cancellationToken)
    {
        // Copia antes de limpar (evita modificar coleção durante iteração).
        var events = sale.DomainEvents.ToList();
        sale.ClearDomainEvents();

        foreach (var domainEvent in events)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
