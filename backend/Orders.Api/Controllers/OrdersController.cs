using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Models;
using Orders.Api;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _context;
    private readonly IHubContext<OrdersHub> _hubContext;

    public OrdersController(OrdersDbContext context, IHubContext<OrdersHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] Order order)
    {
        order.Id = Guid.NewGuid();
        order.Status = "Pendente";
        order.DataCriacao = DateTime.UtcNow;

        order.StatusHistories.Add(new OrderStatusHistory
        {
            Status = "Pendente",
            DataAlteracao = DateTime.UtcNow,
            OrderId = order.Id
        });

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("OrderUpdated", order);

        var outboxEvent = new OutboxEvent
        {
            AggregateId = order.Id,
            Type = "OrderCreated",
            Payload = System.Text.Json.JsonSerializer.Serialize(order),
            CorrelationId = order.Id.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        _context.OutboxEvents.Add(outboxEvent);
        await _context.SaveChangesAsync();

        // var message = new ServiceBusMessage(order.Id.ToString())
        // {
        //     CorrelationId = order.Id.ToString(),
        //     Subject = "OrderCreated"
        // };

        // await _serviceBusSender.SendMessageAsync(message);

        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
    }

    [HttpPost("{id}/notify")]
    public async Task<IActionResult> NotifyOrder(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.StatusHistories)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        Console.WriteLine($"Enviando evento OrderUpdated para o SignalR: {order.Id}");
        await _hubContext.Clients.All.SendAsync("OrderUpdated", order);

        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var order = await _context.Orders
                                  .Include(o => o.StatusHistories)
                                  .FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _context.Orders.Include(o => o.StatusHistories).ToListAsync();
        return Ok(orders);
    }
}
