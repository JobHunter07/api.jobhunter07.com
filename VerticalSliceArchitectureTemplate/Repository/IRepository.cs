using System.Data.Common;

namespace VerticalSliceArchitectureTemplate.Repository;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    IEnumerable<T> ExecuteQuery(string query, DbParameter[] dbParams);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    void RemoveRange(IEnumerable<T> entities);
}