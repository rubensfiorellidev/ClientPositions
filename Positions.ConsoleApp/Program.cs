using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Positions.ConsoleApp.Contracts;
using Positions.ConsoleApp.Data;
using Positions.ConsoleApp.ExternalServices;
using Positions.ConsoleApp.Imports;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {

        var cs = context.Configuration.GetConnectionString("NPSqlConnection");
        services.AddPooledDbContextFactory<PositionsDbContext>(opts =>
        {
            opts.UseNpgsql(cs, npgsql => npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(2), null));
            opts.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.Configure<ExternalApiOptions>(context.Configuration.GetSection("ExternalApi"));

        var ext = context.Configuration.GetSection("ExternalApi").Get<ExternalApiOptions>() ?? new();
        if (ext.UseMock || string.IsNullOrWhiteSpace(ext.Key))
        {
            services.AddSingleton<IPositionsSource>(_ => new MockPositionsSource(
                total: 25_000, positions: 10_000, products: 100, clients: 300, days: 365, seed: 42));
        }
        else
        {
            services.AddHttpClient<ApiPositionsSource>()
                    .AddResilienceHandler("extapi", b =>
                    {
                        b.AddRetry(new HttpRetryStrategyOptions
                        {
                            MaxRetryAttempts = 5,
                            BackoffType = DelayBackoffType.Exponential,
                            UseJitter = true,
                            Delay = TimeSpan.FromSeconds(2)
                        });
                        b.AddTimeout(TimeSpan.FromSeconds(30));
                    });

            services.AddSingleton<IPositionsSource>(sp => sp.GetRequiredService<ApiPositionsSource>());
        }

        services.AddSingleton<IPositionsImporter, PositionsImporter>();
        services.AddSingleton<IPositionsDbContextFactory, PositionsDbContextFactoryWrapper>();

    });

var app = builder.Build();
using var scope = app.Services.CreateScope();
var importer = scope.ServiceProvider.GetRequiredService<IPositionsImporter>();
await importer.ImportAsync(CancellationToken.None);
