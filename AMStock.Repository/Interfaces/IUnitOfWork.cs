using System;
using System.Threading.Tasks;
using AMStock.Core.Models;

namespace AMStock.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        void Commit();
        Task<int> CommitAync();
        //void Dispose();        
        void Dispose(bool disposing);
        IRepository<TEntity> Repository<TEntity>() where TEntity : EntityBase;
    }
}
