using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Azure.Messaging.ServiceBus;
using Orders.Api.Models;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _context;
    private readonly ServiceBusSender _serviceBusSender;

    public OrdersController(OrdersDbContext context, ServiceBusClient client)
    {
        _context = context;
        _serviceBusSender = client.CreateSender("orders-queue");
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] Order order)
    {
        order.Id = Guid.NewGuid();
        order.Status = "Pendente";
        order.DataCriacao = DateTime.UtcNow;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Enviar mensagem para Service Bus
        var message = new ServiceBusMessage(System.Text.Json.JsonSerializer.Serialize(order))
        {
            CorrelationId = order.Id.ToString(),
            Subject = "OrderCreated"
        };
        await _serviceBusSender.SendMessageAsync(message);

        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
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
