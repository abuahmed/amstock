using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using AMStock.Core.Models;
using AMStock.Core;
using System.Data.Common;

namespace AMStock.DAL
{
    public class AMStockServerDBContext : DbContextBase
    {
        public AMStockServerDBContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AMStockServerDBContext, ServerConfiguration>());
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

    public class ServerDbContextFactory : IDbContextFactory<AMStockServerDBContext>
    {
        public AMStockServerDBContext Create()
        {
            #region For Debug
            const string serverName = ".";
            const string initialCatalog = "AMStockDbWeb1";
            const string sQlServConString = "data source=" + serverName + ";initial catalog=" + initialCatalog + ";" +
                                            "user id=sa;password=amihan";
            Singleton.ConnectionStringName = sQlServConString;
            Singleton.ProviderName = "System.Data.SqlClient";
            var sql = new SqlConnectionFactory(sQlServConString);
            return new AMStockServerDBContext(sql.CreateConnection(sQlServConString), true); 
            #endregion

            #region For Release
            //const string serverIp = "198.38.83.33";
            //const string serverInitialCatalog = "ibrahim11_amstock1";
            //var sQlServerConString = "Data Source=" + serverIp + ";Initial Catalog=" + serverInitialCatalog + ";"+
            //                          "User ID=ibrahim11_armsdev;Password=@rmsd3v;"+
            //                          "encrypt=true;trustServerCertificate=true";

            //var sql = new SqlConnectionFactory(sQlServerConString);
            //return new AMStockServerDBContext(sql.CreateConnection(sQlServerConString), true); 
            #endregion
        }
    }

    public class ServerConfiguration : DbMigrationsConfiguration<AMStockServerDBContext>
    {
        public ServerConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(AMStockServerDBContext context)
        {
            if (Singleton.SeedDefaults)
            {
                var client = context.Set<SettingDTO>().Find(1);// context.Set<ClientDTO>().Find(1);
                if (client == null)
                {
                    context = (AMStockServerDBContext)DbContextUtil.Seed(context);
                }
            }
            base.Seed(context);
        }
    }
}
