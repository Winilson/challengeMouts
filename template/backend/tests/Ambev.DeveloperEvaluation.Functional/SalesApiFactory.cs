using Ambev.DeveloperEvaluation.ORM;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

public class SalesApiFactory : WebApplicationFactory<Ambev.DeveloperEvaluation.WebApi.Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:13")
        .WithDatabase("developer_evaluation")
        .WithUsername("developer")
        .WithPassword("ev@luAt10n")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _postgres.ExecScriptAsync("CREATE EXTENSION IF NOT EXISTS pgcrypto;");
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DefaultContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<DefaultContext>(options =>
                options.UseNpgsql(
                    _postgres.GetConnectionString(),
                    b => b.MigrationsAssembly("Ambev.DeveloperEvaluation.ORM")
                )
            );
        });
    }
}