using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using TechCase.FundTransfer.Core.Domain;
using TechCase.FundTransfer.Core.Interfaces;

namespace TechCase.FundTransfer.Infrastructure.Queue
{
    internal class RabbitMqServices : IPublisher
    {
        public void Publish(Event eventMessage)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using IConnection connection = factory.CreateConnection();
            using IModel channel = connection.CreateModel();
            channel.QueueDeclare(eventMessage.Subject, durable: false, exclusive: false, autoDelete: false, arguments: null);
            var strMessage = JsonSerializer.Serialize(eventMessage);
            var msgBody = Encoding.UTF8.GetBytes(strMessage);
            IBasicProperties properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            channel.BasicPublish(exchange: "", routingKey: eventMessage.Subject, basicProperties: properties, body: msgBody);
        }
    }
}
