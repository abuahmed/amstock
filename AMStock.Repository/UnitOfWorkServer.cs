using System;
using AMStock.DAL.Interfaces;

namespace AMStock.Repository
{
    public class UnitOfWorkServer : UnitOfWorkCommon
    {
        public UnitOfWorkServer(IDbContext dbContext)
        {
            if (dbContext == null)
                throw new ArgumentNullException("context");

            Context = dbContext;
            _instanceId = Guid.NewGuid();
        }
    }
}