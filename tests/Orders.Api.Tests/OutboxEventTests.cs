using Xunit;
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Models;
using Orders.Api;
using System;
using System.Threading.Tasks;

public class OutboxEventTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pgContainer;
    private OrdersDbContext _dbContext = null!;

    public OutboxEventTests()
    {
        _pgContainer = new PostgreSqlBuilder()
            .WithDatabase("orders")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _pgContainer.StartAsync();

        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql(_pgContainer.GetConnectionString())
            .Options;

        _dbContext = new OrdersDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _pgContainer.DisposeAsync();
    }

    [Fact]
    public async Task DeveSalvarEventoNaOutboxAoCriarPedido()
    {
        var order = new Order
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Cliente = "Teste",
            Produto = "Notebook",
            Valor = 2500,
            Status = "Pendente",
            DataCriacao = DateTime.SpecifyKind(DateTime.Parse("2025-09-21T00:00:00Z"), DateTimeKind.Utc)
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        var outboxEvent = new OutboxEvent
        {
            AggregateId = order.Id,
            Type = "OrderCreated",
            Payload = System.Text.Json.JsonSerializer.Serialize(order),
            CorrelationId = order.Id.ToString(),
            CreatedAt = DateTime.SpecifyKind(DateTime.Parse("2025-09-21T00:00:00Z"), DateTimeKind.Utc)
        };

        _dbContext.OutboxEvents.Add(outboxEvent);
        await _dbContext.SaveChangesAsync();

        var payload = System.Text.Json.JsonSerializer.Serialize(order, new System.Text.Json.JsonSerializerOptions {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        var eventoSalvo = await _dbContext.OutboxEvents
            .FirstOrDefaultAsync(e => e.AggregateId == order.Id && e.Type == "OrderCreated" && e.ProcessedAt == null);

        Assert.NotNull(eventoSalvo);
        Assert.Equal(order.Id, eventoSalvo!.AggregateId);
        Assert.Equal("OrderCreated", eventoSalvo.Type);
        Assert.Null(eventoSalvo.ProcessedAt);

        var actualEvent = System.Text.Json.JsonSerializer.Serialize(eventoSalvo, new System.Text.Json.JsonSerializerOptions {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        var expectedEvent = await File.ReadAllTextAsync(Path.Combine("golden", "outbox_event.json"));
        Assert.Equal(expectedEvent.Trim(), actualEvent.Trim());
    }
}