namespace CRM.Domain.Interfaces.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<TMessage>(string queueName, TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class;
}
