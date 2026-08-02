namespace API.Application.Interfaces;

public interface IMemberUnitOfWork : IUnitOfWorkBase
{
    IMemberRepository MemberRepository { get; }
}
