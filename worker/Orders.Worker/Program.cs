using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // Configuração do DbContext
        services.AddDbContext<OrdersDbContext>(options =>
            options.UseNpgsql(hostContext.Configuration.GetConnectionString("Default")));

        // Registrar Worker
        services.AddHostedService<Worker>();
    })
    .Build();

await builder.RunAsync();

// ---------------------------
// DbContext e entidade mínima
// ---------------------------

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }
    public DbSet<Order> Orders => Set<Order>();
}

public class Order
{
    public Guid Id { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Produto { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Status { get; set; } = "Pendente";
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}

// ---------------------------
// Worker mínimo
// ---------------------------

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker iniciado!");
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker rodando em: {time}", DateTimeOffset.Now);
            await Task.Delay(5000, stoppingToken);
        }
    }
}