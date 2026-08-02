namespace API.Application.Interfaces;

public interface IUnitOfWorkBase
{
    Task<bool> Complete();
}
