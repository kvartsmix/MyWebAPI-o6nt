using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly IServiceProvider Services;

    public BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        Client = factory.CreateClient();
        Services = factory.Services;
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedStorageBoxesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}