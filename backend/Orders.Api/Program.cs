using Microsoft.EntityFrameworkCore;
using Azure.Messaging.ServiceBus;
using Orders.Api.Models;

namespace Orders.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var defaultConnection = builder.Configuration.GetConnectionString("Default");
            builder.Services.AddDbContext<OrdersDbContext>(options =>
                options.UseNpgsql(defaultConnection));

            builder.Services.AddHealthChecks();
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });
            builder.Services.AddSignalR();
            builder.Services.AddCors(options =>
                {
                    var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:3000";
                    options.AddPolicy("AllowFrontend", policy =>
                        policy.WithOrigins(frontendUrl)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials());
                });

            var app = builder.Build();

            app.UseCors("AllowFrontend");

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
                db.Database.Migrate();
            }

            app.MapHub<OrdersHub>("/ordersHub");
            app.MapHealthChecks("/health");
            app.MapControllers();
            app.Run();
        }
    }

    public class OrdersDbContext : DbContext
    {
        public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
        public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();
    }
}