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
            Id = Guid.NewGuid(),
            Cliente = "Teste",
            Produto = "Notebook",
            Valor = 2500,
            Status = "Pendente",
            DataCriacao = DateTime.UtcNow
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        var outboxEvent = new OutboxEvent
        {
            AggregateId = order.Id,
            Type = "OrderCreated",
            Payload = System.Text.Json.JsonSerializer.Serialize(order),
            CorrelationId = order.Id.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.OutboxEvents.Add(outboxEvent);
        await _dbContext.SaveChangesAsync();

        var eventoSalvo = await _dbContext.OutboxEvents
            .FirstOrDefaultAsync(e => e.AggregateId == order.Id && e.Type == "OrderCreated" && e.ProcessedAt == null);

        Assert.NotNull(eventoSalvo);
        Assert.Equal(order.Id, eventoSalvo!.AggregateId);
        Assert.Equal("OrderCreated", eventoSalvo.Type);
        Assert.Null(eventoSalvo.ProcessedAt);
    }
}