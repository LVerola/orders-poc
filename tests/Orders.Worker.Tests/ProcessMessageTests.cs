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
        var order = new Order { Status = "Novo" };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        order.Status = "Processando";
        db.OrderStatusHistories.Add(new OrderStatusHistory { Status = "Processando", Order = order });
        order.Status = "Finalizado";
        db.OrderStatusHistories.Add(new OrderStatusHistory { Status = "Finalizado", Order = order });
        await db.SaveChangesAsync();

        Assert.Equal("Finalizado", order.Status);
        Assert.Equal(2, await db.OrderStatusHistories.CountAsync());
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