using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : BaseEntity
{
    public string SaleNumber { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public Guid BranchId { get; private set; }
    public string BranchName { get; private set; } = string.Empty;
    public bool IsCancelled { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    //Coleção de itens encapsulado
    private readonly List<SaleItem> _items = new();
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    //Propriedade Calculada
    public decimal TotalAmount => _items.Where(i => !i.IsCancelled).Sum(i => i.TotalAmount);

    //Acumulador de domain events
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    //Construtor
    private Sale() { }
    public Sale(
        string saleNumber,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName)
    {
        if (string.IsNullOrWhiteSpace(saleNumber))
            throw new DomainException("O número da venda é obrigatório.");

        if (customerId == Guid.Empty)
            throw new DomainException("O ID do cliente é obrigatório.");

        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("O nome do cliente é obrigatório.");

        if (branchId == Guid.Empty)
            throw new DomainException("O ID da filial é obrigatório.");

        if (string.IsNullOrWhiteSpace(branchName))
            throw new DomainException("O nome da filial é obrigatório.");

        SaleNumber = saleNumber;
        Date = DateTime.UtcNow;
        CustomerId = customerId;
        CustomerName = customerName;
        BranchId = branchId;
        BranchName = branchName;
        IsCancelled = false;
        CreatedAt = DateTime.UtcNow;


        AddDomainEvent(new SaleCreatedEvent(this));
    }

    #region Métodos de negócio

    public void AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        if (IsCancelled)
            throw new DomainException("Não é possível adicionar itens a uma venda cancelada.");

        var existing = _items.FirstOrDefault(i => i.ProductId == productId && !i.IsCancelled);

        if (existing != null)
            throw new DomainException(
                $"O produto '{productName}' já está nesta venda. Cancele o item existente primeiro.");

        var item = new SaleItem(Id, productId, productName, quantity, unitPrice);
        _items.Add(item);

        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainException("A venda já está cancelada.");

        IsCancelled = true;
        UpdatedAt = DateTime.UtcNow;

        foreach (var item in _items.Where(i => !i.IsCancelled))
        {
            item.Cancel();
        }

        AddDomainEvent(new SaleCancelledEvent(this));
    }

    public void CancelItem(Guid itemId)
    {
        if (IsCancelled)
            throw new DomainException("Não é possível cancelar o item: a venda já está cancelada.");

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"O item {itemId} não foi encontrado nesta venda.");

        item.Cancel();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ItemCancelledEvent(this, item));


    }

    public void UpdateBasicInfo(string customerName, string branchName)
    {
        if (IsCancelled)
            throw new DomainException("Não é possível atualizar uma venda cancelada.");

        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("O nome do cliente é obrigatório.");

        if (string.IsNullOrWhiteSpace(branchName))
            throw new DomainException("O nome da filial é obrigatório.");
        CustomerName = customerName;
        BranchName = branchName;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SaleModifiedEvent(this));
    }

    #endregion



    //Gerenciamento de domain events
    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);


    //Validação
    public ValidationResultDetail Validate()
    {
        var validator = new SaleValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}
