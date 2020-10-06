#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AMStock.Core.Models;
using AMStock.Repository.Interfaces;

#endregion

namespace AMStock.Repository
{
    public sealed class RepositoryQuery<TEntity> : IRepositoryQuery<TEntity> where TEntity : EntityBase
    {
        private readonly List<Expression<Func<TEntity, object>>> _includeProperties;
        private readonly Repository<TEntity> _repository;
        private Expression<Func<TEntity, bool>> _filter;
        private readonly List<Expression<Func<TEntity, bool>>> _filterList;
        private Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> _orderByQuerable;
        private int? _page;
        private int? _pageSize;
        private int? _showDeleted;

        public RepositoryQuery(Repository<TEntity> repository)
        {
            _repository = repository;
            _includeProperties = new List<Expression<Func<TEntity, object>>>();
            _filterList = new List<Expression<Func<TEntity, bool>>>();
        }

        public RepositoryQuery<TEntity> Filter(Expression<Func<TEntity, bool>> filter)
        {
            _filter = filter;
            return this;
        }
        public RepositoryQuery<TEntity> FilterList(Expression<Func<TEntity, bool>> filter)
        {
            _filterList.Add(filter);
            return this;
        }
        public RepositoryQuery<TEntity> OrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
        {
            _orderByQuerable = orderBy;
            return this;
        }

        public RepositoryQuery<TEntity> Include(params Expression<Func<TEntity, object>>[] expression)
        {
            foreach (var navigationProperty in expression)
            {
                _includeProperties.Add(navigationProperty);
            }
            //query = query.Include(navigationProperty).Where(t => t.Enabled);
            //_includeProperties.Add(expression);
            return this;
        }

        public IEnumerable<TEntity> GetPage(int page, int pageSize, out int totalCount, int? showDeleted = null)//null=Enabled only, -1=Disabled only, 1=both Enabled and Disabled
        {
            _page = page;
            _pageSize = pageSize;
            totalCount = _repository.Get(_filterList).Count();

            return _repository.Get(_filterList, _orderByQuerable, _includeProperties, _page, _pageSize, _showDeleted);
        }

        public IQueryable<TEntity> Get(int? showDeleted = null)//null=Enabled only, -1=Disabled only, 1=both Enabled and Disabled
        {
            _showDeleted = showDeleted;
            return _repository.Get(_filter, _orderByQuerable, _includeProperties, _page, _pageSize, _showDeleted);
        }
        public IQueryable<TEntity> GetList(int? showDeleted = null)//null=Enabled only, -1=Disabled only, 1=both Enabled and Disabled
        {
            _showDeleted = showDeleted;
            return _repository.Get(_filterList, _orderByQuerable, _includeProperties, _page, _pageSize, _showDeleted);
        }
        public async Task<IEnumerable<TEntity>> GetAsync()
        {
            return await _repository.GetAsync(_filter, _orderByQuerable, _includeProperties, _page, _pageSize);
        }

        public IQueryable<TEntity> SqlQuery(string query, params object[] parameters)
        {
            return _repository.SqlQuery(query, parameters).AsQueryable();
        }
    }
}