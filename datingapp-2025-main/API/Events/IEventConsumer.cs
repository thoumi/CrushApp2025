namespace API.Events;

/// <summary>
/// Interface pour les consommateurs d'événements
/// </summary>
public interface IEventConsumer
{
    Task ConsumeAsync<T>(string queueName, Func<T, Task> handler, CancellationToken cancellationToken);
}


