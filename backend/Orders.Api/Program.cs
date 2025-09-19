using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------
// Configuração do DbContext
// ---------------------------
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// ---------------------------
// Registrar HealthCheck EF Core
// ---------------------------
builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrdersDbContext>("PostgreSQL");

// ---------------------------
// Adicionar suporte a controllers
// ---------------------------
builder.Services.AddControllers();

var app = builder.Build();

// ---------------------------
// Endpoints mínimos
// ---------------------------

// Hello World para teste rápido
app.MapGet("/", () => "Hello World from Orders API!");

// Health endpoint para monitoramento
app.MapHealthChecks("/health");

// Mapear controllers (futuros CRUD endpoints)
app.MapControllers();

app.Run();

// ---------------------------
// DbContext e entidades mínimas
// ---------------------------

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
}

// Entidade de pedido
public class Order
{
    public Guid Id { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Produto { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Status { get; set; } = "Pendente";
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}

// Histórico de status do pedido
public class OrderStatusHistory
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Status { get; set; } = "Pendente";
    public DateTime DataAlteracao { get; set; } = DateTime.UtcNow;
}