namespace SEMS.Application.Abstractions;

public interface IMessageBus
{
    Task PublishAsync(string topic, byte[] payload, CancellationToken cancellationToken = default);
}

