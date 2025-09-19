using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Orders.Api.Mocks
{
    public class MockServiceBusSender : ServiceBusSender
    {
        private readonly string _queueName;

        public MockServiceBusSender(string queueName)
        {
            _queueName = queueName;
        }

        public override async Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken = default)
        {
            // Simula envio da mensagem apenas logando
            Console.WriteLine($"[MOCK SERVICE BUS] Mensagem enviada para {_queueName}: {JsonSerializer.Serialize(message.Body.ToString())}");
            await Task.CompletedTask;
        }
    }

    public class MockServiceBusClient : ServiceBusClient
    {
        public MockServiceBusClient() : base("Endpoint=fake;SharedAccessKeyName=fake;SharedAccessKey=fake") { }

        public override ServiceBusSender CreateSender(string queueName)
        {
            return new MockServiceBusSender(queueName);
        }
    }
}
