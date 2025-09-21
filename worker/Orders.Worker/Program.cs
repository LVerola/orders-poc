using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
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
    });

await builder.Build().RunAsync();

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
}