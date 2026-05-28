using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.CreateSale;

public static class CreateSaleHandlerTestData
{
    private static readonly Faker Faker = new("pt_BR");

    public static CreateSaleCommand GenerateValidCommand(int itemCount = 2)
    {
        var items = new List<CreateSaleItemDto>();
        for (int i = 0; i < itemCount; i++)
        {
            items.Add(new CreateSaleItemDto
            {
                ProductId = Guid.NewGuid(),
                ProductName = Faker.Commerce.ProductName(),
                Quantity = Faker.Random.Int(1, 5),
                UnitPrice = Faker.Random.Decimal(1, 50)
            });
        }

        return new CreateSaleCommand
        {
            SaleNumber = $"SALE-{Faker.Random.Int(1000, 9999)}",
            CustomerId = Guid.NewGuid(),
            CustomerName = Faker.Person.FullName,
            BranchId = Guid.NewGuid(),
            BranchName = $"Filial {Faker.Address.City()}",
            Items = items
        };
    }
}
