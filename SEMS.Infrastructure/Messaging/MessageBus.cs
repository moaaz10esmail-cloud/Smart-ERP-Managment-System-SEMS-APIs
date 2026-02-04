using RabbitMQ.Client;
using SEMS.Application.Abstractions;

namespace SEMS.Infrastructure.Messaging;

public class RabbitMqBus : IMessageBus
{
    private readonly ConnectionFactory _factory;
    public RabbitMqBus(string connectionString)
    {
        _factory = new ConnectionFactory { Uri = new Uri(connectionString) };
    }
    public Task PublishAsync(string topic, byte[] payload, CancellationToken cancellationToken = default)
    {
        using var conn = _factory.CreateConnection();
        using var chan = conn.CreateModel();
        chan.ExchangeDeclare(topic, ExchangeType.Fanout, durable: true);
        chan.BasicPublish(exchange: topic, routingKey: "", basicProperties: null, body: payload);
        return Task.CompletedTask;
    }
}

public class InMemoryBus : IMessageBus
{
    public Task PublishAsync(string topic, byte[] payload, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
