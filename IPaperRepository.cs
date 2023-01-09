using GenericRepositoryManager;
using GenericRepositoryManager.Model;

namespace Services.Paper.Repositories;

public interface IPaperRepository : IRepository<Model.Paper>
{
    Task<PagingResponse<IEnumerable<Model.Paper>>> GetAllWithPagingAsync(Guid id, int currentPageNumber, int pageSize,
        string? search);
}