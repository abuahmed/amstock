using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using AMStock.Core;
using AMStock.Core.Models.Interfaces;
using AMStock.DAL.Interfaces;
using AMStock.Core.Models;
using AMStock.Repository.Interfaces;
using System.Linq.Expressions;
using AutoMapper;

namespace AMStock.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : EntityBase
    {
        private readonly IDbContext _context;
        private readonly DbSet<TEntity> _dbSet;
        private readonly Guid _instanceId;

        public Repository(IDbContext context)
        {
            if (context != null)
            {
                _context = context;
                _dbSet = context.Set<TEntity>();
            }
            _instanceId = Guid.NewGuid();
        }

        public Guid InstanceId
        {
            get { return _instanceId; }
        }

        public virtual TEntity FindById(object id)
        {
            return id != null ? _dbSet.Find(id) != null && _dbSet.Find(id).Enabled ? _dbSet.Find(id) : null : null;
        }
  
        public virtual async Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            if (_dbSet != null)
                if (keyValues != null)
                {
                    var ent = await _dbSet.FindAsync(cancellationToken, keyValues);
                    if (ent != null && ent.Enabled)
                        return ent;
                }

            return null;
        }
        public virtual async Task<TEntity> FindAsync(params object[] keyValues)
        {
            if (_dbSet != null)
                if (keyValues != null)
                {
                    var ent = await _dbSet.FindAsync(keyValues);
                    if (ent != null && ent.Enabled)
                        return ent;
                }
            return null;
        }
        public virtual IQueryable<TEntity> SqlQuery(string query, params object[] parameters)
        {
            if (_dbSet != null) if (query != null)
                    if (parameters != null) return _dbSet.SqlQuery(query, parameters).AsQueryable();
            return null;
        }

        public virtual void Insert(TEntity entity)
        {
            if (_dbSet != null) if (entity != null) _dbSet.Add(entity);
        }
        public virtual void InsertRange(IEnumerable<TEntity> entities)
        {
            if (entities != null)
                foreach (var entity in entities)
                    if (entity != null) Insert(entity);
        }
        public virtual void InsertGraph(TEntity entity)
        {
            if (_dbSet != null) if (entity != null) _dbSet.Add(entity);
        }
        public virtual void InsertGraphRange(IEnumerable<TEntity> entities)
        {
            if (_dbSet != null) if (entities != null)
                    _dbSet.AddRange(entities);
        }

        public virtual void SimpleUpdate(TEntity entity)
        {
            if (_dbSet != null)
                if (entity != null)
                    if (_context != null)
                    {
                        _dbSet.Add(entity);
                        _context.Entry(entity).State = EntityState.Modified;
                    }
        }
        public virtual void Update(TEntity entity)
        {
            if (_dbSet != null)
                if (entity != null)
                    if (_context != null)
                    {
                        if (entity.RowGuid == (Guid.Parse("00000000-0000-0000-0000-000000000000")))
                            entity.RowGuid = InstanceId;

                        entity.ModifiedByUserId = Singleton.User != null ? Singleton.User.UserId : 1;
                        entity.DateLastModified = DateTime.Now;

                        _dbSet.Add(entity);
                        _context.Entry(entity).State = EntityState.Modified;
                    }
        }
        public virtual void InsertUpdate(TEntity entity)
        {
            if (_dbSet != null)
                if (entity != null)
                    if (_context != null)
                    {
                        if (entity.Id == 0)
                        {
                            Insert(entity);
                        }
                        else
                        {
                            _dbSet.Add(entity);
                            _context.Entry(entity).State = EntityState.Modified;
                        }
                    }
        }

        public virtual TEntity FindByRowGuid(Guid rowGuid)
        {
            if (_dbSet != null)
            {
                IEnumerable<TEntity> query = _dbSet;
                var firstOrDefault = query.FirstOrDefault(e => e.RowGuid == rowGuid);
                return FindById(firstOrDefault != null ? firstOrDefault.Id : 0);
            }
            return null;
        }
        public virtual void CrudByRowGuid(TEntity entity)
        {
            if (_dbSet != null)
                if (entity != null)
                    if (_context != null)
                    {
                        var ent = FindByRowGuid(entity.RowGuid);

                        if (ent == null)
                        {
                            Insert(entity);
                        }
                        else
                        {
                            //if (entity.RowGuid == (Guid.Parse("00000000-0000-0000-0000-000000000000")))
                            //    entity.RowGuid = InstanceId;

                            var itId = ent.Id;
                            Mapper.Reset();
                            Mapper.CreateMap<TEntity, TEntity>();
                            ent = Mapper.Map(entity, ent);

                            ent.Id = itId;

                            _dbSet.Add(ent);
                            _context.Entry(ent).State = EntityState.Modified;
                        }
                    }
        }

        public virtual void Delete(object id)
        {
            if (_dbSet != null)
                if (id != null)
                {
                    var entity = _dbSet.Find(id);
                    Delete(entity);
                }
        }
        public virtual void Delete(TEntity entity)
        {
            if (_dbSet != null)
                if (entity != null)
                {
                    ((IObjectState)entity).ObjectState = ObjectState.Deleted;
                    _dbSet.Attach(entity);
                    _dbSet.Remove(entity);
                }
        }

        public virtual IRepositoryQuery<TEntity> Query()
        {
            var repositoryGetFluentHelper = new RepositoryQuery<TEntity>(this);
            return repositoryGetFluentHelper;
        }
        internal IQueryable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            List<Expression<Func<TEntity, object>>> includeProperties = null,
            int? page = null, int? pageSize = null, int? showdeleted = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (showdeleted == null)//null=Enabled only, -1=Disabled only, 1=both Enabled and Disabled
                query = query.Where(q => q.Enabled);
            else if (showdeleted == -1)
                query = query.Where(q => !q.Enabled);

            if (includeProperties != null)
                includeProperties.ForEach(i =>
                {
                    query = query.Include(i);
                });

            if (filter != null)
                query = query.Where(filter);

            if (orderBy != null)
                query = orderBy(query);

            if (page != null && pageSize != null)
                query = query
                    .Skip((page.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);

            return query;
        }
        internal IQueryable<TEntity> Get(
           List<Expression<Func<TEntity, bool>>> filter = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
           List<Expression<Func<TEntity, object>>> includeProperties = null,
           int? page = null, int? pageSize = null, int? showdeleted = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (showdeleted == null)//null=Enabled only, -1=Disabled only, 1=both Enabled and Disabled
                query = query.Where(q => q.Enabled);
            else if (showdeleted == -1)
                query = query.Where(q => !q.Enabled);

            if (includeProperties != null)
                includeProperties.ForEach(i =>
                {
                    query = query.Include(i);
                });

            if (filter != null)
                filter.ForEach(i =>
                {
                    query = query.Where(i);
                });

            if (orderBy != null)
                query = orderBy(query);

            if (page != null && pageSize != null)
                query = query
                    .Skip((page.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);

            return query;
        }
        internal async Task<IEnumerable<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            List<Expression<Func<TEntity, object>>> includeProperties = null,
            int? page = null,
            int? pageSize = null)
        {
            return Get(filter, orderBy, includeProperties, page, pageSize).AsEnumerable();
        }
    }
}
