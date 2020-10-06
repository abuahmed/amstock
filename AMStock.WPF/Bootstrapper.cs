using System;
using AMStock.DAL;
using AMStock.DAL.Interfaces;
using AMStock.WPF.ViewModel;
using Microsoft.Practices.Unity;
using AMStock.Repository.Interfaces;
using AMStock.Repository;
using AMStock.Core;
using AMStock.Core.Enumerations;
using System.IO;

namespace AMStock.WPF
{
    public class Bootstrapper
    {
        public IUnityContainer Container { get; set; }

        public Bootstrapper()
        {
            Container = new UnityContainer();

            ConfigureContainer();
        }

        private void ConfigureContainer()
        {
            switch (Singleton.Edition)
            {
                #region Server/Database Name and Path
                case AMStockEdition.CompactEdition:
                    var path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\PinnaStock\\";
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    var pathfile = Path.Combine(path, "PinnaStockDbProd.sdf");//AMStockDbAbduMu//AMStockDbHalal
                    Singleton.SqlceFileName = pathfile;
                    Singleton.SeedDefaults = true;

                    Container.RegisterType<IDbContext, AMStockDBContext>(new ContainerControlledLifetimeManager());
                    //Container.RegisterInstance<IDbContext>(new DbContextFactory().Create());
                    break;
                case AMStockEdition.ServerEdition:
                    Singleton.SqlceFileName = "AMStockDbNesro2";// "AMStockDb1";
                    Singleton.SeedDefaults = true;

                    Container.RegisterType<IDbContext, AMStockDBContext>(new ContainerControlledLifetimeManager());
                    //Container.RegisterInstance<IDbContext>(new DbContextFactory().Create());
                    break;
                case AMStockEdition.OnlineEdition:
                    Singleton.SqlceFileName = "AMStockDb3";
                    Singleton.SeedDefaults = false;

                    Container.RegisterType<IDbContext, AMStockServerDBContext>(new ContainerControlledLifetimeManager());
                    //Container.RegisterInstance<IDbContext>(new ServerDbContextFactory().Create());
                    break;
                #endregion
            }

            Container.RegisterType<IUnitOfWork, UnitOfWork>();
            //Container.RegisterInstance<IUnitOfWork>(new UnitOfWork(Container.Resolve<IDbContext>()));
            Container.RegisterType<MainViewModel>();
        }
    }
}
