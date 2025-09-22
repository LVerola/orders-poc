using Xunit;
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Models;
using Orders.Api;
using System;
using System.Threading.Tasks;
using System.Linq;

public class OrderIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pgContainer;
    private OrdersDbContext _dbContext = null!;

    public OrderIntegrationTests()
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
    public async Task DeveSalvarPedidoNoBanco()
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

        var pedidoSalvo = await _dbContext.Orders.FindAsync(order.Id);

        Assert.NotNull(pedidoSalvo);
        Assert.Equal("Teste", pedidoSalvo!.Cliente);
    }

    [Fact]
    public async Task DeveListarTodosOsPedidos()
    {
        var order1 = new Order
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Cliente = "Cliente 1",
            Produto = "Produto 1",
            Valor = 1000,
            Status = "Pendente",
            DataCriacao = DateTime.SpecifyKind(DateTime.Parse("2025-09-21T00:00:00Z"), DateTimeKind.Utc)
        };
        var order2 = new Order
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Cliente = "Cliente 2",
            Produto = "Produto 2",
            Valor = 2000,
            Status = "Pendente",
            DataCriacao = DateTime.SpecifyKind(DateTime.Parse("2025-09-21T00:00:00Z"), DateTimeKind.Utc)
        };

        _dbContext.Orders.AddRange(order1, order2);
        await _dbContext.SaveChangesAsync();

        var pedidos = await _dbContext.Orders.OrderBy(o => o.Cliente).ToListAsync();
        var actual = System.Text.Json.JsonSerializer.Serialize(pedidos, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

        var expected = await File.ReadAllTextAsync(Path.Combine("golden", "orders_list.json"));
        Assert.Equal(expected.Trim(), actual.Trim());
    }

    [Fact]
    public async Task DeveRetornarDetalhesDoPedido()
    {
        var order = new Order
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Cliente = "Detalhe",
            Produto = "Produto Detalhe",
            Valor = 1500,
            Status = "Pendente",
            DataCriacao = DateTime.SpecifyKind(DateTime.Parse("2025-09-21T00:00:00Z"), DateTimeKind.Utc)
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        var pedido = await _dbContext.Orders.FindAsync(order.Id);

        var actual = System.Text.Json.JsonSerializer.Serialize(pedido, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        var expected = await File.ReadAllTextAsync(Path.Combine("golden", "order_detail.json"));
        Assert.Equal(expected.Trim(), actual.Trim());
    }

    [Fact]
    public async Task DeveRetornarNullParaPedidoInexistente()
    {
        var pedido = await _dbContext.Orders.FindAsync(Guid.NewGuid());
        Assert.Null(pedido);
    }
}