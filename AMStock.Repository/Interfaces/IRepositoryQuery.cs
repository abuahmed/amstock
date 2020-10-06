using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AMStock.Core.Models;

namespace AMStock.Repository.Interfaces
{
    public interface IRepositoryQuery<TEntity> where TEntity : EntityBase
    {
        RepositoryQuery<TEntity> Filter(Expression<Func<TEntity, bool>> filter);
        RepositoryQuery<TEntity> FilterList(Expression<Func<TEntity, bool>> filter);
        RepositoryQuery<TEntity> OrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy);
        RepositoryQuery<TEntity> Include(params Expression<Func<TEntity, object>>[] expression);
        IEnumerable<TEntity> GetPage(int page, int pageSize, out int totalCount,int? showDeleted = null);
        IQueryable<TEntity> Get(int? showDeleted = null);
        IQueryable<TEntity> GetList(int? showDeleted = null);
        Task<IEnumerable<TEntity>> GetAsync();
        IQueryable<TEntity> SqlQuery(string query, params object[] parameters);
    }
}