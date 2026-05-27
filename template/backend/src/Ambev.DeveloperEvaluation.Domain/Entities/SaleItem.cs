using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;


public class SaleItem : BaseEntity
{
    public const int MaxQuantityPerProduct = 20;
    public const int Tier1MinQuantity = 4;
    public const int Tier2MinQuantity = 10;
    public const decimal Tier1DiscountRate = 0.10m;
    public const decimal Tier2DiscountRate = 0.20m;

    public Guid SaleId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }

    private SaleItem() { }

    public SaleItem(Guid saleId, Guid productId, string productName, int quantity, decimal unitPrice)
    {

        ValidateProductName(productName);
        ValidateQuantity(quantity);
        ValidateUnitPrice(unitPrice);

        SaleId = saleId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        IsCancelled = false;

        CalculateAmounts();
    }

    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainException("O item já está cancelado.");

        IsCancelled = true;
    }

    public static decimal CalculateDiscountRate(int quantity)
    {

        if (quantity > MaxQuantityPerProduct)
            return 0m;

        if (quantity >= Tier2MinQuantity)
            return Tier2DiscountRate;

        if (quantity >= Tier1MinQuantity)
            return Tier1DiscountRate;

        return 0m;
    }

    private void CalculateAmounts()
    {
        var subtotal = Quantity * UnitPrice;
        var rate = CalculateDiscountRate(Quantity);
        Discount = subtotal * rate;
        TotalAmount = subtotal - Discount;
    }

    private static void ValidateProductName(string productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("O nome do produto é obrigatório.");
    }

    private static void ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("A quantidade deve ser maior que zero.");

        if (quantity > MaxQuantityPerProduct)
            throw new DomainException(
                $"Não é permitido vender mais de {MaxQuantityPerProduct} itens iguais por produto.");
    }

    private static void ValidateUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
            throw new DomainException("O preço unitário não pode ser negativo.");
    }
}
