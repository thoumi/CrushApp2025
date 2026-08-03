namespace API.Application.Interfaces;

/// <summary>
/// Union de toutes les briques de compétence de l'application. Les consommateurs
/// ne doivent PAS dépendre de cette interface directement : chacun dépend de la
/// brique (IMemberUnitOfWork, IMessagingUnitOfWork, ILikesUnitOfWork,
/// IMatchingUnitOfWork, IModerationUnitOfWork...) qui couvre exactement son besoin.
/// </summary>
public interface IUnitOfWork : IMessagingUnitOfWork, IMatchingUnitOfWork, IModerationUnitOfWork
{
    bool HasChanges();
}
