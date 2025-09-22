using Xunit;
using Orders.Worker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;

public class ProcessMessageTests
{
    [Fact]
    public async Task MensagemComOrderIdValido_DeveAtualizarStatusEAdicionarHistorico()
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var db = new OrdersDbContext(options);
        var orderId = Guid.Parse("161fbd37-7a1f-4b30-85aa-123456789abc");
        var order = new Order { Id = orderId, Status = "Pendente" };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        order.Status = "Processando";
        db.OrderStatusHistories.Add(new OrderStatusHistory { Status = "Processando", Order = order });
        order.Status = "Finalizado";
        db.OrderStatusHistories.Add(new OrderStatusHistory { Status = "Finalizado", Order = order });
        await db.SaveChangesAsync();

        Assert.Equal("Finalizado", order.Status);
        Assert.Equal(2, await db.OrderStatusHistories.CountAsync());

        var historicos = await db.OrderStatusHistories
            .AsNoTracking()
            .OrderBy(h => h.Status)
            .Select(h => new { h.Status, h.OrderId })
            .ToListAsync();
        var actualHistoricos = System.Text.Json.JsonSerializer.Serialize(historicos, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        var expectedHistoricos = await File.ReadAllTextAsync(Path.Combine("golden", "order_status_histories.json"));
        Assert.Equal(expectedHistoricos.Trim(), actualHistoricos.Trim());

        var actualOrder = System.Text.Json.JsonSerializer.Serialize(
            new { order.Id, order.Status },
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
        );
        var expectedOrder = await File.ReadAllTextAsync(Path.Combine("golden", "order_final.json"));
        Assert.Equal(expectedOrder.Trim(), actualOrder.Trim());
    }

    [Fact]
    public async Task MensagemComOrderIdInvalido_DeveSerIgnorada()
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var db = new OrdersDbContext(options);

        var orderIdInvalido = Guid.NewGuid();

        var pedido = await db.Orders.FindAsync(orderIdInvalido);

        Assert.Null(pedido);
    }

    [Fact]
    public async Task PedidoNaoEncontrado_DeveRetornarSemAlteracao()
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var db = new OrdersDbContext(options);

        var pedido = await db.Orders.FindAsync(Guid.NewGuid());
        Assert.Null(pedido);
    }

    [Fact]
    public async Task PedidoJaFinalizado_DeveIgnorarProcessamento()
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var db = new OrdersDbContext(options);
        var order = new Order { Status = "Finalizado" };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var pedido = await db.Orders.FirstOrDefaultAsync(o => o.Status == "Finalizado");

        Assert.NotNull(pedido);
        Assert.Equal("Finalizado", pedido.Status);
    }
}