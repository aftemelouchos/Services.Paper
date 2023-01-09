using GenericRepositoryManager;

namespace Services.Paper.Repositories
{
    public interface IUnitOfWork
    {
        IPaperRepository Paper { get; }
        IYoksisRepository Yoksis { get; }
        IUserService User { get; }
    }
}
