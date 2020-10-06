using System;
using AMStock.DAL.Interfaces;


namespace AMStock.Repository
{
    public class UnitOfWork : UnitOfWorkCommon
    {
        public UnitOfWork(IDbContext dbContext)
        {
            if (dbContext == null)
                throw new ArgumentNullException("context");

            Context = dbContext;
            _instanceId = Guid.NewGuid();
        }
    }
}
