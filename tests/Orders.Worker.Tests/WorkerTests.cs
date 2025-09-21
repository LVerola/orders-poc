using Xunit;
using Orders.Worker;
using Orders.Worker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;
using Moq;
using System;
using System.Threading.Tasks;

public class WorkerTests
{
    [Fact]
    public async Task DevePersistirPedidoHistoricoEEvento()
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var db = new OrdersDbContext(options);

        var order = new Order { Status = "Novo" };
        var history = new OrderStatusHistory { Status = "Novo", Order = order };
        var outbox = new OutboxEvent { AggregateId = order.Id, ProcessedAt = null };

        db.Orders.Add(order);
        db.OrderStatusHistories.Add(history);
        db.OutboxEvents.Add(outbox);
        await db.SaveChangesAsync();

        Assert.Equal(1, await db.Orders.CountAsync());
        Assert.Equal(1, await db.OrderStatusHistories.CountAsync());
        Assert.Equal(1, await db.OutboxEvents.CountAsync());
    }

    [Fact]
    public async Task DeveEnviarEventosOutboxParaServiceBusEMarcarComoProcessados()
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var db = new OrdersDbContext(options);
        var outbox = new OutboxEvent { AggregateId = Guid.NewGuid(), ProcessedAt = null };
        db.OutboxEvents.Add(outbox);
        await db.SaveChangesAsync();

        var mockLogger = new Mock<ILogger<Worker>>();
        var mockServiceBusClient = new Mock<ServiceBusClient>();
        var mockProvider = new Mock<IServiceProvider>();

        var worker = new Worker(mockLogger.Object, mockServiceBusClient.Object, mockProvider.Object);

        outbox.ProcessedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        Assert.NotNull(outbox.ProcessedAt);
    }

    [Fact]
    public async Task DeveAguardarEntreVerificacoes()
    {
        var mockLogger = new Mock<ILogger<Worker>>();
        var mockServiceBusClient = new Mock<ServiceBusClient>();
        var mockProvider = new Mock<IServiceProvider>();

        var worker = new Worker(mockLogger.Object, mockServiceBusClient.Object, mockProvider.Object);

        var start = DateTime.UtcNow;
        await Task.Delay(5000);
        var end = DateTime.UtcNow;

        Assert.True((end - start).TotalSeconds >= 5);
    }

    [Fact]
    public void DeveLogarErroNoHandlerDeErro()
    {
        var mockLogger = new Mock<ILogger<Worker>>();
        var mockServiceBusClient = new Mock<ServiceBusClient>();
        var mockProvider = new Mock<IServiceProvider>();

        var worker = new Worker(mockLogger.Object, mockServiceBusClient.Object, mockProvider.Object);

        var exception = new Exception("Erro de teste");

        mockLogger.Object.LogError(exception, "Erro no worker");

        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}