using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web.Security;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.DAL.Interfaces;
using AMStock.DAL.Mappings;
using WebMatrix.WebData;

namespace AMStock.DAL
{
    public static class DbContextUtil
    {
        public static DbModelBuilder OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new ProductActivationMap());
            modelBuilder.Configurations.Add(new SettingMap());
            modelBuilder.Configurations.Add(new BuildVersionMap());
            modelBuilder.Configurations.Add(new ErrorLogMap());

            modelBuilder.Configurations.Add(new AddressMap());
            modelBuilder.Configurations.Add(new BusinessPartnerAddressMap());
            modelBuilder.Configurations.Add(new CategoryMap());

            modelBuilder.Configurations.Add(new ContactMap());
            modelBuilder.Configurations.Add(new BusinessPartnerContactMap());

            modelBuilder.Configurations.Add(new ItemMap());
            modelBuilder.Configurations.Add(new ItemQuantityMap());
            modelBuilder.Configurations.Add(new ItemBorrowMap());

            modelBuilder.Configurations.Add(new ClientMap());
            modelBuilder.Configurations.Add(new OrganizationMap());
            modelBuilder.Configurations.Add(new FinancialAccountMap());
            modelBuilder.Configurations.Add(new BankGuarnteeMap());
            modelBuilder.Configurations.Add(new WarehouseMap());
            modelBuilder.Configurations.Add(new BusinessPartnerMap());
            modelBuilder.Configurations.Add(new SalesPersonMap());

            modelBuilder.Configurations.Add(new TransactionHeaderMap());
            modelBuilder.Configurations.Add(new TransactionLineMap());

            modelBuilder.Configurations.Add(new ItemsMovementHeaderMap());
            modelBuilder.Configurations.Add(new ItemsMovementLineMap());

            modelBuilder.Configurations.Add(new PaymentMap());
            modelBuilder.Configurations.Add(new CheckMap());
            modelBuilder.Configurations.Add(new PaymentClearanceMap());

            modelBuilder.Configurations.Add(new PhysicalInventoryHeaderMap());
            modelBuilder.Configurations.Add(new PhysicalInventoryLineMap());

            modelBuilder.Configurations.Add(new UserMap());
            modelBuilder.Configurations.Add(new MembershipMap());
            modelBuilder.Configurations.Add(new RoleMap());

            modelBuilder.Configurations.Add(new CpoMap());
            modelBuilder.Configurations.Add(new SmtpServerMap());

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            return modelBuilder;
        }

        public static IDbContext Seed(IDbContext context)
        {
            #region Setting Seeds
            context.Set<SettingDTO>().Add(new SettingDTO()
            {
                Id = 1,
                CheckCreditLimit = false,
                HandleBankTransaction = false,
                TaxType = TaxTypes.NoTax,
                TaxPercent = 0
            });
            #endregion

            #region Business Partner Seeds
            if (Singleton.Edition != AMStockEdition.OnlineEdition)
            {
                #region Customer
                var bp = new BusinessPartnerDTO()
                {
                    Id = 1,
                    BusinessPartnerType = BusinessPartnerTypes.Customer,
                    Code = "C0001",
                    DisplayName = "_Walking Customer",
                    CreditLimit = 0,
                    MaxNoCreditTransactions = 0,
                    PaymentTerm = 0,
                    AllowCreditsWithoutCheck = false,
                };

                context.Set<BusinessPartnerAddressDTO>().Add(new BusinessPartnerAddressDTO
                 {
                     Address = CommonUtility.GetDefaultAddress(),
                     BusinessPartner = bp
                 });

                context.Set<BusinessPartnerContactDTO>().Add(new BusinessPartnerContactDTO
                {
                    Contact = new ContactDTO()
                    {
                        FullName = "ibra yas",
                        Address = CommonUtility.GetDefaultAddress(),
                    },
                    BusinessPartner = bp
                });
                #endregion

                #region Supplier

                var sbp = new BusinessPartnerDTO()
                {
                    BusinessPartnerType = BusinessPartnerTypes.Supplier,
                    Code = "S0001",
                    DisplayName = "_Common Supplier",
                    CreditLimit = 0,
                    MaxNoCreditTransactions = 0,
                    PaymentTerm = 0,
                    AllowCreditsWithoutCheck = true,
                };

                context.Set<BusinessPartnerAddressDTO>().Add(new BusinessPartnerAddressDTO
                {
                    Address = CommonUtility.GetDefaultAddress(),
                    BusinessPartner = sbp

                });

                context.Set<BusinessPartnerContactDTO>().Add(new BusinessPartnerContactDTO
                {
                    Contact = new ContactDTO()
                    {
                        FullName = "ibra yas",
                        Address = CommonUtility.GetDefaultAddress(),
                    },
                    BusinessPartner = sbp
                });
                #endregion
            }

            #endregion

            #region List Seeds
            context.Set<CategoryDTO>().Add(new CategoryDTO { NameType = NameTypes.Category, DisplayName = "No Category" });

            context.Set<CategoryDTO>().Add(new CategoryDTO { NameType = NameTypes.UnitMeasure, DisplayName = "Pcs" });
            #endregion

            #region Bank Seeds
            context.Set<CategoryDTO>().Add(new CategoryDTO { NameType = NameTypes.Bank, DisplayName = "Commercial Bank of Ethiopia" });
            context.Set<CategoryDTO>().Add(new CategoryDTO { NameType = NameTypes.Bank, DisplayName = "Dashen Bank" });
            context.Set<CategoryDTO>().Add(new CategoryDTO { NameType = NameTypes.Bank, DisplayName = "Awash Bank" });
            context.Set<CategoryDTO>().Add(new CategoryDTO { NameType = NameTypes.Bank, DisplayName = "United Bank" });
            context.Set<CategoryDTO>().Add(new CategoryDTO { NameType = NameTypes.Bank, DisplayName = "Nib Bank" });
            context.Set<CategoryDTO>().Add(new CategoryDTO { NameType = NameTypes.Bank, DisplayName = "Buna International Bank" });
            context.Set<CategoryDTO>().Add(new CategoryDTO { NameType = NameTypes.Bank, DisplayName = "Wegagen Bank" });
            context.Set<CategoryDTO>().Add(new CategoryDTO { NameType = NameTypes.Bank, DisplayName = "Abyssinia Bank" });
            context.Set<CategoryDTO>().Add(new CategoryDTO { NameType = NameTypes.Bank, DisplayName = "Abay Bank" });
            context.Set<CategoryDTO>().Add(new CategoryDTO { NameType = NameTypes.Bank, DisplayName = "Birhan Bank" });
            context.Set<CategoryDTO>().Add(new CategoryDTO { NameType = NameTypes.Bank, DisplayName = "Enat Bank" });
            context.Set<CategoryDTO>().Add(new CategoryDTO { NameType = NameTypes.Bank, DisplayName = "Lion Bank" });
            #endregion

            return context;
        }

        public static IDbContext GetDbContextInstance()
        {
            switch (Singleton.Edition)
            {
                case AMStockEdition.CompactEdition:
                    return new DbContextFactory().Create();
                case AMStockEdition.ServerEdition:
                    return new DbContextFactory().Create();
                case AMStockEdition.OnlineEdition:
                    return new ServerDbContextFactory().Create();
            }
            return new DbContextFactory().Create();
        }
    }
}