using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;

namespace TechCase.FundTransfer.Infrastructure.Queue
{
    // https://github.com/rabbitmq/rabbitmq-dotnet-client
    public abstract class ConsumerBase
    {
        private readonly ManualResetEvent _resetEvent = null;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public ConsumerBase(ManualResetEvent resetEvent)
        {
            _resetEvent = resetEvent;
            var connectionFactory = new ConnectionFactory() { HostName = "localhost" };
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }
        protected virtual int AttempLimit => 1;
        public abstract string QueueName { get; }
        public abstract void EventReceivedHandler(object model, BasicDeliverEventArgs deliveryEventArgs);

        public void ConsumeQueue()
        {
            _channel.QueueDeclare(QueueName, durable: false, exclusive: false, autoDelete: false, null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += EventReceivedHandler;

            _channel.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer);

            _resetEvent.WaitOne();
        }
    }
}
