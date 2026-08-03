namespace API.Application.Interfaces;

public interface IMessagingUnitOfWork : IMemberUnitOfWork
{
    IMessageRepository MessageRepository { get; }
}
