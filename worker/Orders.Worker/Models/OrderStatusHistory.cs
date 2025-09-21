using System.Text.Json.Serialization;

namespace Orders.Worker.Models
{
    public class OrderStatusHistory
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string Status { get; set; } = "Pendente";
        public DateTime DataAlteracao { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public Order Order { get; set; } = null!;
    }
}
