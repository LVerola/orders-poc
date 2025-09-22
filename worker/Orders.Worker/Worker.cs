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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor = _busClient.CreateProcessor("orders-queue", new ServiceBusProcessorOptions());
        _processor.ProcessMessageAsync += ProcessMessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;

        await _processor.StartProcessingAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
            var busClient = scope.ServiceProvider.GetRequiredService<ServiceBusClient>();
            var sender = busClient.CreateSender("orders-queue");

            var events = await db.OutboxEvents
                .Where(e => e.ProcessedAt == null)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync(stoppingToken);

            foreach (var evt in events)
            {
                var message = new ServiceBusMessage(evt.AggregateId.ToString())
                {
                    CorrelationId = evt.CorrelationId,
                    Subject = evt.Type
                };

                await sender.SendMessageAsync(message, stoppingToken);

                evt.ProcessedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("Evento Outbox {Id} enviado ao Service Bus.", evt.Id);
            }

            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
    {
        _logger.LogInformation("Processando mensagem da fila...");
        var orderIdString = args.Message.Body.ToString();
        using var httpClient = new HttpClient();
        var apiUrl = Environment.GetEnvironmentVariable("API_URL");

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

        if (order.Status == "Finalizado")
        {
            _logger.LogInformation("Order {orderId} já está finalizado, ignorando.", order.Id);
            await args.CompleteMessageAsync(args.Message);
            return;
        }

        order.Status = "Processando";
        order.StatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = "Processando"
        });
        await db.SaveChangesAsync();
        await httpClient.PostAsync($"{apiUrl}/orders/{order.Id}/notify", null);

        await Task.Delay(5000);

        order.Status = "Finalizado";
        order.StatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = "Finalizado"
        });
        await db.SaveChangesAsync();
        await httpClient.PostAsync($"{apiUrl}/orders/{order.Id}/notify", null);

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
}
