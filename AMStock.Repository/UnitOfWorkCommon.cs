using System;
using System.Collections;
using System.Data.Entity;
using System.Threading.Tasks;
using AMStock.Core.Models;
using AMStock.DAL.Interfaces;
using AMStock.Repository.Interfaces;

namespace AMStock.Repository
{
    public class UnitOfWorkCommon : IUnitOfWork
    {
        protected IDbContext Context;
        private Hashtable _repositories;
        protected Guid _instanceId;
        
        public Guid InstanceId
        {
            get { return _instanceId; }
        }

        internal DbSet<T> GetDbSet<T>() where T : class
        {
            return Context.Set<T>();
        }

        public void Commit()
        {
            Context.SaveChanges();
        }
        public async Task<int> CommitAync()
        {
            return await Context.SaveChangesAsync();
        }
        
        #region Dispose
        private bool _disposed;
        public virtual void Dispose(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                    Context.Dispose();

            _disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        } 
        #endregion

        public IRepository<T> Repository<T>() where T : EntityBase
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(Repository<>);

                var repositoryInstance =
                    Activator.CreateInstance(repositoryType
                        .MakeGenericType(typeof(T)), Context);

                _repositories.Add(type, repositoryInstance);
            }

            return (IRepository<T>)_repositories[type];
        }
    }
}