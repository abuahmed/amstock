using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.Core;
using System.Data.Common;

namespace AMStock.DAL
{
    public class AMStockDBContext : DbContextBase
    {
        public AMStockDBContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AMStockDBContext, Configuration>());
            Configuration.ProxyCreationEnabled = false;
        }
        public new DbSet<T> Set<T>() where T : class
        {
            return base.Set<T>();
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            DbContextUtil.OnModelCreating(modelBuilder);
        }
    }

    public class DbContextFactory : IDbContextFactory<AMStockDBContext>
    {
        public AMStockDBContext Create()
        {
            switch (Singleton.Edition)
            {
                case AMStockEdition.CompactEdition:
                    var sqlCeConString = "Data Source=" + Singleton.SqlceFileName + ";" +
                                         "Max Database Size=4091;Password=amSt0ckP@ssw0rd";
                    Singleton.ConnectionStringName = sqlCeConString;
                    Singleton.ProviderName = "System.Data.SqlServerCe.4.0";
                    var sqlce = new SqlCeConnectionFactory(Singleton.ProviderName);
                    return new AMStockDBContext(sqlce.CreateConnection(sqlCeConString), true);

                case AMStockEdition.ServerEdition:
                    const string serverName = "."; // "MasServerPc";
                    var sQlServConString = "data source=" + serverName + ";initial catalog=" + Singleton.SqlceFileName +
                                              ";user id=sa;password=amihan";
                    Singleton.ConnectionStringName = sQlServConString;
                    Singleton.ProviderName = "System.Data.SqlClient";
                    var sql = new SqlConnectionFactory(sQlServConString);
                    return new AMStockDBContext(sql.CreateConnection(sQlServConString), true);
            }
            return null;
        }
    }

    public class Configuration : DbMigrationsConfiguration<AMStockDBContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            
        }

        protected override void Seed(AMStockDBContext context)
        {
            if (Singleton.SeedDefaults)
            {
                var setting = context.Set<SettingDTO>().Find(1);
                if (setting == null)
                {
                    context = (AMStockDBContext)DbContextUtil.Seed(context);
                }
            }
            base.Seed(context);
        }
    }
}
