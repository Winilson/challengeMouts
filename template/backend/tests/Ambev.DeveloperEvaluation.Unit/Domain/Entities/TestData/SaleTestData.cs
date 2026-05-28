using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

public static class SaleTestData
{
    private static readonly Faker Faker = new("pt_BR");

    public static Sale GenerateValidSale()
    {
        var sale = new Sale(
            saleNumber: $"SALE-{Faker.Random.Int(1000, 9999)}",
            customerId: Guid.NewGuid(),
            customerName: Faker.Person.FullName,
            branchId: Guid.NewGuid(),
            branchName: $"Filial {Faker.Address.City()}")
        {
            Id = Guid.NewGuid()
        };
        return sale;
    }

    public static Sale GenerateSaleWithItems(int itemCount = 3)
    {
        var sale = GenerateValidSale();

        for (int i = 0; i < itemCount; i++)
        {
            sale.AddItem(
                productId: Guid.NewGuid(),
                productName: Faker.Commerce.ProductName(),
                quantity: Faker.Random.Int(1, 5),
                unitPrice: Faker.Random.Decimal(1, 100));
        }

        foreach (var item in sale.Items)
            item.Id = Guid.NewGuid();

        return sale;
    }
}
