using Microsoft.EntityFrameworkCore;
using Azure.Messaging.ServiceBus;
using Orders.Api.Models;
using Orders.Api.Mocks;

var builder = WebApplication.CreateBuilder(args);

// Se não houver a variável real do Azure Service Bus, usa o mock
var serviceBusConnectionString = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTIONSTRING");

if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
{
    Console.WriteLine("SERVICEBUS_CONNECTIONSTRING não encontrada. Usando MockServiceBus para desenvolvimento.");
    builder.Services.AddSingleton<ServiceBusClient, MockServiceBusClient>();
}
else
{
    builder.Services.AddSingleton<ServiceBusClient>(_ =>
        new ServiceBusClient(serviceBusConnectionString));
}

// Configura DbContext usando connection string do appsettings ou variáveis de ambiente
var defaultConnection = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseNpgsql(defaultConnection));

builder.Services.AddHealthChecks();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.WriteIndented = true; // opcional, facilita leitura
        });

var app = builder.Build();

// Aplica migrations automaticamente
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.Migrate();
}

// app.MapGet("/", () => "Hello World from Orders API!");
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();

// DbContext
public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
}
