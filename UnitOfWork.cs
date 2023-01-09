using ConfigurationManager.Model;
using GenericRepositoryManager;
using GenericRepositoryManager.Context;

namespace Services.Paper.Repositories
{
    public class UnitOfWork: IUnitOfWork
    {
        public UnitOfWork()
        {
            Paper = new PaperRepository(DbContext.ConnectionString);
            Yoksis = new YoksisRepository(ConfigContext.YoksisBaseUrl);
            User = new UserService(ConfigContext.gRPCUserService);
        }
        public IPaperRepository Paper { get; private set; }
        public IYoksisRepository Yoksis { get; private set; }
        public IUserService User { get; private set; }
    }
}
