namespace Orders.Api.Models
{
    public class OrderStatusHistory
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string Status { get; set; } = "Pendente";
        public DateTime DataAlteracao { get; set; } = DateTime.UtcNow;

        // Propriedade de navegação
        public Order Order { get; set; } = null!;
    }
}
