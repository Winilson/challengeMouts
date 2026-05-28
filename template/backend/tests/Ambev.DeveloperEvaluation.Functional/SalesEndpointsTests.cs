using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

public class SalesEndpointsTests : IClassFixture<SalesApiFactory>
{
    private readonly HttpClient _client;

    public SalesEndpointsTests(SalesApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static object BuildValidPayload(string? saleNumber = null) => new
    {
        saleNumber = saleNumber ?? $"SALE-{Guid.NewGuid():N}".Substring(0, 30),
        customerId = Guid.NewGuid(),
        customerName = "João Silva",
        branchId = Guid.NewGuid(),
        branchName = "Filial BH Centro",
        items = new[]
        {
            new { productId = Guid.NewGuid(), productName = "Cerveja Brahma", quantity = 5, unitPrice = 4.50m },
            new { productId = Guid.NewGuid(), productName = "Refrigerante Coca", quantity = 12, unitPrice = 9.90m }
        }
    };

    private static Guid ExtractId(JsonElement root)
    {
        if (root.TryGetProperty("data", out var data))
        {
            if (data.TryGetProperty("id", out var idDirect))
                return idDirect.GetGuid();

            if (data.TryGetProperty("data", out var dataInner) &&
                dataInner.TryGetProperty("id", out var idNested))
                return idNested.GetGuid();
        }
        throw new InvalidOperationException("ID não encontrado na resposta: " + root.GetRawText());
    }

    private static bool ExtractIsCancelled(JsonElement root)
    {
        if (root.TryGetProperty("data", out var data))
        {
            if (data.TryGetProperty("isCancelled", out var direct))
                return direct.GetBoolean();

            if (data.TryGetProperty("data", out var dataInner) &&
                dataInner.TryGetProperty("isCancelled", out var nested))
                return nested.GetBoolean();
        }
        throw new InvalidOperationException("isCancelled não encontrado na resposta.");
    }

    [Fact(DisplayName = "POST /api/Sales: valid payload returns 201 Created")]
    public async Task CreateSale_ValidPayload_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/api/Sales", BuildValidPayload());
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact(DisplayName = "GET /api/Sales/{id}: returns 200 after POST")]
    public async Task GetSaleById_AfterCreate_Returns200()
    {
        var post = await _client.PostAsJsonAsync("/api/Sales", BuildValidPayload());
        post.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await post.Content.ReadFromJsonAsync<JsonElement>();
        var id = ExtractId(created);

        var get = await _client.GetAsync($"/api/Sales/{id}");

        get.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "POST /api/Sales: quantity over 20 returns 400")]
    public async Task CreateSale_QuantityOver20_Returns400()
    {
        var payload = new
        {
            saleNumber = $"SALE-OVER-{Guid.NewGuid():N}".Substring(0, 30),
            customerId = Guid.NewGuid(),
            customerName = "Cliente",
            branchId = Guid.NewGuid(),
            branchName = "Filial",
            items = new[]
            {
                new { productId = Guid.NewGuid(), productName = "P", quantity = 25, unitPrice = 1m }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/Sales", payload);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "POST /api/Sales: duplicate saleNumber returns 409")]
    public async Task CreateSale_DuplicateSaleNumber_Returns409()
    {
        var saleNumber = $"DUP-{Guid.NewGuid():N}".Substring(0, 30);

        var first = await _client.PostAsJsonAsync("/api/Sales", BuildValidPayload(saleNumber));
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await _client.PostAsJsonAsync("/api/Sales", BuildValidPayload(saleNumber));
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "GET /api/Sales/{id}: non-existent returns 404")]
    public async Task GetSaleById_NonExistent_Returns404()
    {
        var response = await _client.GetAsync($"/api/Sales/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "DELETE /api/Sales/{id}: cancels the sale")]
    public async Task DeleteSale_Returns200()
    {
        var post = await _client.PostAsJsonAsync("/api/Sales", BuildValidPayload());
        var created = await post.Content.ReadFromJsonAsync<JsonElement>();
        var id = ExtractId(created);

        var delete = await _client.DeleteAsync($"/api/Sales/{id}");
        delete.StatusCode.Should().Be(HttpStatusCode.OK);

        // Confirma via GET que ficou cancelada
        var get = await _client.GetAsync($"/api/Sales/{id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await get.Content.ReadFromJsonAsync<JsonElement>();
        ExtractIsCancelled(body).Should().BeTrue();
    }

    [Fact(DisplayName = "GET /api/Sales: returns paginated list (200)")]
    public async Task ListSales_Returns200()
    {
        await _client.PostAsJsonAsync("/api/Sales", BuildValidPayload());

        var response = await _client.GetAsync("/api/Sales?_page=1&_size=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}