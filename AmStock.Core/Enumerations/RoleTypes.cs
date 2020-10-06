using System.ComponentModel;

namespace AMStock.Core.Enumerations
{
    public enum RoleTypes
    {
        //[Description("System Administration")]
        //Admin,

        [Description("Client Profile Mgmt")]
        ClientProfile,
        [Description("Organization Profile Mgmt")]
        OrganizationProfile,
        [Description("Add Stores/Shops")]
        AddStores,

        #region Options
        //[Description("Options")]
        //Settings,
        [Description("Advanced Options")]
        AdvancedSettings,
        [Description("Tax Options")]
        TaxSettings,
        [Description("General Options")]
        GeneralSettings, 
        #endregion
        
        [Description("Update Stores/Shops")]
        UpdateStores,
        [Description("Add/Update Bank Accounts")]
        AddEditBankAccount,
        [Description("Users Mgt")]
        UsersMgmt,
        [Description("Users Privilege Mgmt")]
        UsersPrivilegeMgmt,
        [Description("Backup and Restore Mgmt")]
        BackupRestore,
        

        #region IOH
        [Description("OnHand Inventories Mgmt")]
        OnHandInventory,
        [Description("Reserve Items")]
        ReserveItems,
        [Description("Add/Update Items Inventory")]
        ItemsQuantity,
        #endregion

        #region PI
        //[Description("Physical Inventories Mgmt.")]
        //PhysicalInventory,
        [Description("View Physical Inventories")]
        ViewPhysicalInventory,
        [Description("Add/Update Physical Inventories")]
        AddPhysicalInventory,
        [Description("Post Physical Inventories")]
        PostPhysicalInventory,
        [Description("View Items Pi Lines")]
        PiLinesHistory,
        #endregion

        #region Business Partners
        [Description("Customers Mgmt")]
        Customers,
        [Description("Customer's Advanced Setting Mgmt")]
        CustomersAdvanced,
        [Description("Suppliers Mgmt")]
        Suppliers,
        #endregion

        #region Items
        [Description("Add/Update Items Data")]
        Items,
        [Description("Add/Update Item Categories")]
        ItemCategories,
        [Description("Add/Update Item UOM")]
        ItemUoms,
        [Description("Import Items")]
        ImportItems,
        #endregion

        #region Sales
        //[Description("Sales")]
        //Sales,
        [Description("Viewing Sales List")]
        ViewSales,
        [Description("Adding/Saving Sales")]
        AddSales,
        [Description("Posting Sales")]
        PostSales,
        [Description("UnPosting Sales")]
        UnPostSales,
        [Description("Deleting Sales")]
        DeleteSales,
        [Description("View Items Sales Lines History")]
        SalesLineHistory,
        [Description("Sales Payments Mgmt")]
        SalesPayments,
        [Description("Print Attachments")]
        PrintAttachments,
        #endregion

        #region Purchases
        //[Description("Purchase")]
        //Purchase,
        [Description("Viewing Purchase List")]
        ViewPurchase,
        [Description("Adding/Saving Purchases")]
        AddPurchase,
        [Description("Posting Purchases")]
        PostPurchase,
        [Description("UnPosting Purchases")]
        UnPostPurchase,
        [Description("Deleting Purchases")]
        DeletePurchase,
        [Description("View Items Purchase Lines History")]
        PurchaseLinesHistory,
        [Description("Purchase Payments Mgmt")]
        PurchasePayments,
        #endregion

        #region Payments
        //[Description("Payments")]
        //Payments,
        [Description("Adding Checks")]
        AddChecks,
        [Description("Deposit Payments Mgmt")]
        DepositPayments,
        [Description("Clear Payments Mgmt")]
        ClearPayments,
        [Description("Expenses and Loans Mgmt")]
        ExpensesLoans,
        #endregion

        #region Extras
        [Description("CPO Mgmt")]
        Cpo,
        [Description("Item Borrowing Mgmt")]
        ItemBorrows,

        [Description("Viewing Reports")]
        ViewReports,
        [Description("Send Emails")]
        SendEmail,
        #endregion
    }
}
