using Microsoft.EntityFrameworkCore;
using Azure.Messaging.ServiceBus;
using Orders.Api.Models;
using Orders.Api.Mocks;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
            policy.WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod());
    });

var app = builder.Build();

app.UseCors("AllowFrontend");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.Migrate();
}

app.MapHealthChecks("/health");
app.MapControllers();
app.Run();

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
}
