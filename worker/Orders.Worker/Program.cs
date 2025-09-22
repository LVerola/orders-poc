using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Orders.Worker;
using Orders.Worker.Models;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var defaultConnection = hostContext.Configuration.GetConnectionString("Default");
        services.AddDbContext<OrdersDbContext>(options =>
            options.UseNpgsql(defaultConnection));

        var sbConnection = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTIONSTRING");
        services.AddSingleton(new ServiceBusClient(sbConnection));

        services.AddHostedService<Worker>();

        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Orders.Worker"))
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddJaegerExporter(options =>
                        {
                            options.AgentHost = Environment.GetEnvironmentVariable("JAEGER_HOST") ?? "jaeger";
                            options.AgentPort = int.TryParse(Environment.GetEnvironmentVariable("JAEGER_PORT"), out var port) ? port : 6831;
                        })
                    .AddSource("Orders.Worker");
            });
    });

await builder.Build().RunAsync();

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();
}