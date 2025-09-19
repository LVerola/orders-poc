using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Orders.Worker.Models;

namespace Orders.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ServiceBusClient _busClient;
    private readonly IServiceProvider _services;
    private ServiceBusProcessor? _processor;

    public Worker(ILogger<Worker> logger, ServiceBusClient busClient, IServiceProvider services)
    {
        _logger = logger;
        _busClient = busClient;
        _services = services;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _processor = _busClient.CreateProcessor("orders-queue", new ServiceBusProcessorOptions());
        _processor.ProcessMessageAsync += ProcessMessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;

        await _processor.StartProcessingAsync(cancellationToken);
        _logger.LogInformation("Worker iniciado e ouvindo a fila orders-queue...");
    }

    private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
    {
        var orderIdString = args.Message.Body.ToString();

        if (!Guid.TryParse(orderIdString, out var orderId))
        {
            _logger.LogWarning("OrderId inválido: {orderIdString}", orderIdString);
            await args.DeadLetterMessageAsync(args.Message, "Invalid OrderId");
            return;
        }

        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        var order = await db.Orders
                            .Include(o => o.StatusHistories)
                            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return;

        // Idempotência
        if (order.Status == "Finalizado")
        {
            _logger.LogInformation("Order {orderId} já está finalizado, ignorando.", order.Id);
            await args.CompleteMessageAsync(args.Message);
            return;
        }

        // Atualiza para Processando
        order.Status = "Processando";
        order.StatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = "Processando"
        });
        await db.SaveChangesAsync();

        // Simula processamento
        await Task.Delay(5000);

        // Atualiza para Finalizado
        order.Status = "Finalizado";
        order.StatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = "Finalizado"
        });
        await db.SaveChangesAsync();

        _logger.LogInformation("Order {orderId} finalizado.", order.Id);

        await args.CompleteMessageAsync(args.Message);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Erro no Service Bus");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor != null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
}
