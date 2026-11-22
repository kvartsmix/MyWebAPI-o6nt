using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;
using MongoDB.Driver;

namespace IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private MongoDbRunner? _runner;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {

            _runner = MongoDbRunner.Start();

            var mongoDescriptors = services
                .Where(s => s.ServiceType == typeof(IMongoClient) || s.ServiceType == typeof(IMongoDatabase))
                .ToList();

            foreach (var descriptor in mongoDescriptors)
                services.Remove(descriptor);

            var testClient = new MongoClient(_runner.ConnectionString);
            var testDb = testClient.GetDatabase("mydatabase");
            MyWebAPI.Data.MongoDbClient.UseTestDatabase(testDb);

            services.AddSingleton<IMongoClient>(testClient);
            services.AddSingleton<IMongoDatabase>(testDb);

            services.AddSingleton<DatabaseSeeder>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _runner?.Dispose();
    }
}