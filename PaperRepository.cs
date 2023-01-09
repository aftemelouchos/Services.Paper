using System.Data;
using System.Data.SqlClient;
using Dapper;
using GenericRepositoryManager;
using GenericRepositoryManager.Model;

namespace Services.Paper.Repositories;

public class PaperRepository : GenericRepository<Model.Paper>, IPaperRepository
{
    private readonly string _connectionString;

    public PaperRepository(string connectionString) : base(connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection Connection => new SqlConnection(_connectionString);

    public async Task<PagingResponse<IEnumerable<Model.Paper>>> GetAllWithPagingAsync(Guid id, int currentPageNumber,
        int pageSize, string? search)
    {
        const int maxPagSize = 100;
        pageSize = pageSize is > 0 and <= maxPagSize ? pageSize : maxPagSize;

        var skip = (currentPageNumber - 1) * pageSize;
        var take = pageSize;
        var totalCount = 0;

        using var connection = Connection;

        var whereClause = string.IsNullOrEmpty(search) ? "" : " and (r.PaperName like @search)";

        var query = "WITH LST AS(SELECT * FROM [Paper] as r WHERE UserId = @id" + whereClause + ")" +
                    @"SELECT 
                                        LST.*, t.Total
                                    FROM LST
                                        CROSS JOIN (SELECT Count(*) AS Total FROM LST) AS t
                                    ORDER BY CreatedTime desc " +
                    "OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY;";

        var result = await connection.QueryAsync<Model.Paper, int, Model.Paper>(query,
            (p, c) =>
            {
                totalCount = c;
                return p;
            }, new {id, skip, pageSize, search = "%" + search + "%"}, splitOn: "Total");

        var data = new PagingResponse<IEnumerable<Model.Paper>>(result, totalCount, currentPageNumber, pageSize);
        return data;
    }
}