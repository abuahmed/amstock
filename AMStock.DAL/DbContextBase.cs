using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Threading;
using System.Threading.Tasks;
using AMStock.Core.Models.Interfaces;
using AMStock.DAL.Interfaces;
using System.Data.Common;

namespace AMStock.DAL
{
    public class DbContextBase : DbContext, IDbContext
    {
        private readonly Guid _instanceId;

        public DbContextBase(string nameOrConnectionString) :
            base(nameOrConnectionString)
        {
            _instanceId = Guid.NewGuid();
            Configuration.LazyLoadingEnabled = false;
        }

        public DbContextBase(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
            _instanceId = Guid.NewGuid();
            Configuration.LazyLoadingEnabled = false;
        }

        public Guid InstanceId
        {
            get { return _instanceId; }
        }

        public new DbSet<T> Set<T>() where T : class
        {
            return base.Set<T>();
        }

        public override int SaveChanges()
        {
            try
            {
                // Your code...

                var changes = base.SaveChanges();
                return changes;
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException + ex.Message);
                throw;
            }

        }

        //public override int SaveChanges()
        //{
        //    SyncObjectsStatePreCommit();
        //    var changes = base.SaveChanges();
        //    SyncObjectsStatePostCommit();
        //    return changes;
        //}

        public override Task<int> SaveChangesAsync()
        {
            //SyncObjectsStatePreCommit();
            var changesAsync = base.SaveChangesAsync();
            //SyncObjectsStatePostCommit();
            return changesAsync;
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            //SyncObjectsStatePreCommit();
            var changesAsync = base.SaveChangesAsync(cancellationToken);
            //SyncObjectsStatePostCommit();
            return changesAsync;
        }

        public void SyncObjectState(object entity)
        {
            Entry(entity).State = StateHelper.ConvertState(((IObjectState)entity).ObjectState);
        }

        //private void SyncObjectsStatePreCommit()
        //{
        //    foreach (var dbEntityEntry in ChangeTracker.Entries())
        //        dbEntityEntry.State = StateHelper.ConvertState(((IObjectState)dbEntityEntry.Entity).ObjectState);
        //}

        //private void SyncObjectsStatePostCommit()
        //{
        //    foreach (var dbEntityEntry in ChangeTracker.Entries())
        //        ((IObjectState)dbEntityEntry.Entity).ObjectState = StateHelper.ConvertState(dbEntityEntry.State);
        //}
    }
}
