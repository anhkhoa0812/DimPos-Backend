using System.Linq.Expressions;
using DimPos.MenuCombo.Infrastructure.Filter;
using DimPos.MenuCombo.Infrastructure.Paginate.Interface;
using Microsoft.EntityFrameworkCore.Query;

namespace DimPos.MenuCombo.Infrastructure.Repositories.Interface;

public interface IGenericRepository<T> : IDisposable where T : class
{
    #region Get

    Task<T> SingleOrDefaultAsync(
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);

    Task<TResult> SingleOrDefaultAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);

    Task<ICollection<T>> GetListAsync(
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);

    Task<ICollection<TResult>> GetListAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);

    Task<IPaginate<T>> GetPagingListAsync(
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
        int page = 1,
        int size = 10);
    
    Task<IPaginate<TResult>> GetPagingListAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        IFilter<T> filter = null,
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
        int page = 1,
        int size = 10,
        string sortBy = null,
        bool isAsc = true
    );

    #endregion

    #region Insert

    Task InsertAsync(T entity);

    Task InsertRangeAsync(IEnumerable<T> entities);

    #endregion

    #region Update

    void UpdateAsync(T entity);

    void UpdateRange(IEnumerable<T> entities);

    #endregion

    #region Delete

    void DeleteAsync(T entity);
    void DeleteRangeAsync(IEnumerable<T> entities);

    #endregion
}