using AMStock.Core.Enumerations;
using AMStock.Core.Models;

namespace AMStock.Core.Common
{
    public class UserRolesModel : EntityBase
    {
        //private readonly int _userId;
        public UserRolesModel()
        {
            //_userId = userId;int userId

            #region Role Visibilities
            //Admin = CommonUtility.UserHasRole(user.Id,RoleTypes.Admin) ? "Visible" : "Collapsed";

            ClientProfile = CommonUtility.UserHasRole(RoleTypes.ClientProfile) ? "Visible" : "Collapsed";
            OrganizationProfile = CommonUtility.UserHasRole(RoleTypes.OrganizationProfile) ? "Visible" : "Collapsed";
            AddStores = CommonUtility.UserHasRole(RoleTypes.AddStores) ? "Visible" : "Collapsed";

            //Settings = CommonUtility.UserHasRole(user.Id,RoleTypes.Settings) ? "Visible" : "Collapsed";
            AdvancedSettings = CommonUtility.UserHasRole(RoleTypes.AdvancedSettings) ? "Visible" : "Collapsed";
            TaxSettings = CommonUtility.UserHasRole(RoleTypes.TaxSettings) ? "Visible" : "Collapsed";
            GeneralSettings = CommonUtility.UserHasRole(RoleTypes.GeneralSettings) ? "Visible" : "Collapsed";

            UpdateStores = CommonUtility.UserHasRole(RoleTypes.UpdateStores) ? "Visible" : "Collapsed";
            AddEditBankAccount = CommonUtility.UserHasRole(RoleTypes.AddEditBankAccount) ? "Visible" : "Collapsed";
            UsersMgmt = CommonUtility.UserHasRole(RoleTypes.UsersMgmt) ? "Visible" : "Collapsed";
            UsersPrivilegeMgmt = CommonUtility.UserHasRole(RoleTypes.UsersPrivilegeMgmt) ? "Visible" : "Collapsed";
            BackupRestore = CommonUtility.UserHasRole(RoleTypes.BackupRestore) ? "Visible" : "Collapsed";

            OnHandInventory = CommonUtility.UserHasRole(RoleTypes.OnHandInventory) ? "Visible" : "Collapsed";
            ReserveItems = CommonUtility.UserHasRole(RoleTypes.ReserveItems) ? "Visible" : "Collapsed";
            ItemsQuantity = CommonUtility.UserHasRole(RoleTypes.ItemsQuantity) ? "Visible" : "Collapsed";

            //PhysicalInventory = CommonUtility.UserHasRole(user.Id,RoleTypes.PhysicalInventory) ? "Visible" : "Collapsed";
            ViewPhysicalInventory = CommonUtility.UserHasRole(RoleTypes.ViewPhysicalInventory) ? "Visible" : "Collapsed";
            AddPhysicalInventory = CommonUtility.UserHasRole(RoleTypes.AddPhysicalInventory) ? "Visible" : "Collapsed";
            PostPhysicalInventory = CommonUtility.UserHasRole(RoleTypes.PostPhysicalInventory) ? "Visible" : "Collapsed";
            PiLinesHistory = CommonUtility.UserHasRole(RoleTypes.PiLinesHistory) ? "Visible" : "Collapsed";

            Customers = CommonUtility.UserHasRole(RoleTypes.Customers) ? "Visible" : "Collapsed";
            CustomersAdvanced = CommonUtility.UserHasRole(RoleTypes.CustomersAdvanced) ? "Visible" : "Collapsed";
            Suppliers = CommonUtility.UserHasRole(RoleTypes.Suppliers) ? "Visible" : "Collapsed";

            Items = CommonUtility.UserHasRole(RoleTypes.Items) ? "Visible" : "Collapsed";
            ItemCategories = CommonUtility.UserHasRole(RoleTypes.ItemCategories) ? "Visible" : "Collapsed";
            ItemUoms = CommonUtility.UserHasRole(RoleTypes.ItemUoms) ? "Visible" : "Collapsed";
            ImportItems = CommonUtility.UserHasRole(RoleTypes.ImportItems) ? "Visible" : "Collapsed";

            //Sales = CommonUtility.UserHasRole(user.Id,RoleTypes.Sales) ? "Visible" : "Collapsed";
            ViewSales = CommonUtility.UserHasRole(RoleTypes.ViewSales) ? "Visible" : "Collapsed";
            AddSales = CommonUtility.UserHasRole(RoleTypes.AddSales) ? "Visible" : "Collapsed";
            PostSales = CommonUtility.UserHasRole(RoleTypes.PostSales) ? "Visible" : "Collapsed";
            UnPostSales = CommonUtility.UserHasRole(RoleTypes.UnPostSales) ? "Visible" : "Collapsed";
            DeleteSales = CommonUtility.UserHasRole(RoleTypes.DeleteSales) ? "Visible" : "Collapsed";
            SalesLineHistory = CommonUtility.UserHasRole(RoleTypes.SalesLineHistory) ? "Visible" : "Collapsed";
            SalesPayments = CommonUtility.UserHasRole(RoleTypes.SalesPayments) ? "Visible" : "Collapsed";
            PrintAttachments = CommonUtility.UserHasRole(RoleTypes.PrintAttachments) ? "Visible" : "Collapsed";

            //Purchase = CommonUtility.UserHasRole(user.Id,RoleTypes.Purchase) ? "Visible" : "Collapsed";
            ViewPurchase = CommonUtility.UserHasRole(RoleTypes.ViewPurchase) ? "Visible" : "Collapsed";
            AddPurchase = CommonUtility.UserHasRole(RoleTypes.AddPurchase) ? "Visible" : "Collapsed";
            PostPurchase = CommonUtility.UserHasRole(RoleTypes.PostPurchase) ? "Visible" : "Collapsed";
            UnPostPurchase = CommonUtility.UserHasRole(RoleTypes.UnPostPurchase) ? "Visible" : "Collapsed";
            DeletePurchase = CommonUtility.UserHasRole(RoleTypes.DeletePurchase) ? "Visible" : "Collapsed";
            PurchaseLinesHistory = CommonUtility.UserHasRole(RoleTypes.PurchaseLinesHistory) ? "Visible" : "Collapsed";
            PurchasePayments = CommonUtility.UserHasRole(RoleTypes.PurchasePayments) ? "Visible" : "Collapsed";

            //Payments = CommonUtility.UserHasRole(user.Id,RoleTypes.Payments) ? "Visible" : "Collapsed";
            AddChecks = CommonUtility.UserHasRole(RoleTypes.AddChecks) ? "Visible" : "Collapsed";
            DepositPayments = CommonUtility.UserHasRole(RoleTypes.DepositPayments) ? "Visible" : "Collapsed";
            ClearPayments = CommonUtility.UserHasRole(RoleTypes.ClearPayments) ? "Visible" : "Collapsed";
            ExpensesLoans = CommonUtility.UserHasRole(RoleTypes.ExpensesLoans) ? "Visible" : "Collapsed";

            Cpo = CommonUtility.UserHasRole(RoleTypes.Cpo) ? "Visible" : "Collapsed";
            ItemBorrows = CommonUtility.UserHasRole(RoleTypes.ItemBorrows) ? "Visible" : "Collapsed";
            SendEmail = CommonUtility.UserHasRole(RoleTypes.SendEmail) ? "Visible" : "Collapsed";
            ViewReports = CommonUtility.UserHasRole(RoleTypes.ViewReports) ? "Visible" : "Collapsed";
            #endregion
        }

        #region Public Properties

        //public IList<UserRoleModel> RolesList
        //{
        //    get { return GetValue(() => RolesList); }
        //    set { SetValue(() => RolesList, value); }
        //}

        public string Admin
        {
            get { return GetValue(() => Admin); }
            set { SetValue(() => Admin, value); }
        }

        public string ClientProfile
        {
            get { return GetValue(() => ClientProfile); }
            set { SetValue(() => ClientProfile, value); }
        }
        public string OrganizationProfile
        {
            get { return GetValue(() => OrganizationProfile); }
            set { SetValue(() => OrganizationProfile, value); }
        }
        public string AddStores
        {
            get { return GetValue(() => AddStores); }
            set { SetValue(() => AddStores, value); }
        }

        public string Settings
        {
            get { return GetValue(() => Settings); }
            set { SetValue(() => Settings, value); }
        }
        public string AdvancedSettings
        {
            get { return GetValue(() => AdvancedSettings); }
            set { SetValue(() => AdvancedSettings, value); }
        }
        public string TaxSettings
        {
            get { return GetValue(() => TaxSettings); }
            set { SetValue(() => TaxSettings, value); }
        }
        public string GeneralSettings
        {
            get { return GetValue(() => GeneralSettings); }
            set { SetValue(() => GeneralSettings, value); }
        }

        public string UpdateStores
        {
            get { return GetValue(() => UpdateStores); }
            set { SetValue(() => UpdateStores, value); }
        }
        public string AddEditBankAccount
        {
            get { return GetValue(() => AddEditBankAccount); }
            set { SetValue(() => AddEditBankAccount, value); }
        }
        public string UsersMgmt
        {
            get { return GetValue(() => UsersMgmt); }
            set { SetValue(() => UsersMgmt, value); }
        }
        public string UsersPrivilegeMgmt
        {
            get { return GetValue(() => UsersPrivilegeMgmt); }
            set { SetValue(() => UsersPrivilegeMgmt, value); }
        }
        public string BackupRestore
        {
            get { return GetValue(() => BackupRestore); }
            set { SetValue(() => BackupRestore, value); }
        }

        public string OnHandInventory
        {
            get { return GetValue(() => OnHandInventory); }
            set { SetValue(() => OnHandInventory, value); }
        }
        public string ReserveItems
        {
            get { return GetValue(() => ReserveItems); }
            set { SetValue(() => ReserveItems, value); }
        }
        public string ItemsQuantity
        {
            get { return GetValue(() => ItemsQuantity); }
            set { SetValue(() => ItemsQuantity, value); }
        }

        public string PhysicalInventory
        {
            get { return GetValue(() => PhysicalInventory); }
            set { SetValue(() => PhysicalInventory, value); }
        }
        public string ViewPhysicalInventory
        {
            get { return GetValue(() => ViewPhysicalInventory); }
            set { SetValue(() => ViewPhysicalInventory, value); }
        }
        public string AddPhysicalInventory
        {
            get { return GetValue(() => AddPhysicalInventory); }
            set { SetValue(() => AddPhysicalInventory, value); }
        }
        public string PostPhysicalInventory
        {
            get { return GetValue(() => PostPhysicalInventory); }
            set { SetValue(() => PostPhysicalInventory, value); }
        }
        public string PiLinesHistory
        {
            get { return GetValue(() => PiLinesHistory); }
            set { SetValue(() => PiLinesHistory, value); }
        }

        public string Customers
        {
            get { return GetValue(() => Customers); }
            set { SetValue(() => Customers, value); }
        }
        public string CustomersAdvanced
        {
            get { return GetValue(() => CustomersAdvanced); }
            set { SetValue(() => CustomersAdvanced, value); }
        }
        public string Suppliers
        {
            get { return GetValue(() => Suppliers); }
            set { SetValue(() => Suppliers, value); }
        }

        public string Items
        {
            get { return GetValue(() => Items); }
            set { SetValue(() => Items, value); }
        }
        public string ItemCategories
        {
            get { return GetValue(() => ItemCategories); }
            set { SetValue(() => ItemCategories, value); }
        }
        public string ItemUoms
        {
            get { return GetValue(() => ItemUoms); }
            set { SetValue(() => ItemUoms, value); }
        }
        public string ImportItems
        {
            get { return GetValue(() => ImportItems); }
            set { SetValue(() => ImportItems, value); }
        }

        public string Sales
        {
            get { return GetValue(() => Sales); }
            set { SetValue(() => Sales, value); }
        }
        public string ViewSales
        {
            get
            {
                if (CommonUtility.UserHasRole(RoleTypes.ViewSales))
                    return "Visible";
                else if (PostSales == "Visible" || UnPostSales == "Visible" || DeleteSales == "Visible")
                    return "Visible";
                else
                    return "Collapsed";
            }
            set { SetValue(() => ViewSales, value); }
        }
        public string AddSales
        {
            get { return GetValue(() => AddSales); }
            set { SetValue(() => AddSales, value); }
        }
        public string PostSales
        {
            get { return GetValue(() => PostSales); }
            set { SetValue(() => PostSales, value); SetValue(() => ViewSales, value); }
        }
        public string UnPostSales
        {
            get { return GetValue(() => UnPostSales); }
            set { SetValue(() => UnPostSales, value); SetValue(() => ViewSales, value); }
        }
        public string DeleteSales
        {
            get { return GetValue(() => DeleteSales); }
            set { SetValue(() => DeleteSales, value); SetValue(() => ViewSales, value); }
        }
        public string SalesLineHistory
        {
            get { return GetValue(() => SalesLineHistory); }
            set { SetValue(() => SalesLineHistory, value); }
        }
        public string SalesPayments
        {
            get { return GetValue(() => SalesPayments); }
            set { SetValue(() => SalesPayments, value); }
        }
        public string PrintAttachments
        {
            get { return GetValue(() => PrintAttachments); }
            set { SetValue(() => PrintAttachments, value); }
        }

        public string Purchase
        {
            get { return GetValue(() => Purchase); }
            set { SetValue(() => Purchase, value); }
        }
        public string ViewPurchase
        {
            get
            {
                if (CommonUtility.UserHasRole(RoleTypes.ViewPurchase))
                    return "Visible";
                else if (PostPurchase == "Visible" || UnPostPurchase == "Visible" || DeletePurchase == "Visible")
                    return "Visible";
                else
                    return "Collapsed";
            }
            set { SetValue(() => ViewPurchase, value); }
        }
        public string AddPurchase
        {
            get { return GetValue(() => AddPurchase); }
            set { SetValue(() => AddPurchase, value); }
        }
        public string PostPurchase
        {
            get { return GetValue(() => PostPurchase); }
            set { SetValue(() => PostPurchase, value); SetValue(() => ViewPurchase, value); }
        }
        public string UnPostPurchase
        {
            get { return GetValue(() => UnPostPurchase); }
            set { SetValue(() => UnPostPurchase, value); SetValue(() => ViewPurchase, value); }
        }
        public string DeletePurchase
        {
            get { return GetValue(() => DeletePurchase); }
            set { SetValue(() => DeletePurchase, value); SetValue(() => ViewPurchase, value); }
        }
        public string PurchaseLinesHistory
        {
            get { return GetValue(() => PurchaseLinesHistory); }
            set { SetValue(() => PurchaseLinesHistory, value); }
        }
        public string PurchasePayments
        {
            get { return GetValue(() => PurchasePayments); }
            set { SetValue(() => PurchasePayments, value); }
        }

        public string Payments
        {
            get { return GetValue(() => Payments); }
            set { SetValue(() => Payments, value); }
        }
        public string AddChecks
        {
            get { return GetValue(() => AddChecks); }
            set { SetValue(() => AddChecks, value); }
        }
        public string DepositPayments
        {
            get { return GetValue(() => DepositPayments); }
            set { SetValue(() => DepositPayments, value); }
        }
        public string ClearPayments
        {
            get { return GetValue(() => ClearPayments); }
            set { SetValue(() => ClearPayments, value); }
        }
        public string ExpensesLoans
        {
            get { return GetValue(() => ExpensesLoans); }
            set { SetValue(() => ExpensesLoans, value); }
        }

        public string Cpo
        {
            get { return GetValue(() => Cpo); }
            set { SetValue(() => Cpo, value); }
        }
        public string ItemBorrows
        {
            get { return GetValue(() => ItemBorrows); }
            set { SetValue(() => ItemBorrows, value); }
        }
        public string ViewReports
        {
            get { return GetValue(() => ViewReports); }
            set { SetValue(() => ViewReports, value); }
        }
        public string SendEmail
        {
            get { return GetValue(() => SendEmail); }
            set { SetValue(() => SendEmail, value); }
        }
        #endregion
    }
}