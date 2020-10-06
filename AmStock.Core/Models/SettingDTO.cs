using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AMStock.Core.Enumerations;

namespace AMStock.Core.Models
{
    public class SettingDTO : CommonFieldsA
    {
        #region General Settings
        public bool CheckCreditLimit
        {
            get { return GetValue(() => CheckCreditLimit); }
            set { SetValue(() => CheckCreditLimit, value); }
        }
        public CreditLimitTypes CreditLimitType
        {
            get { return GetValue(() => CreditLimitType); }
            set { SetValue(() => CreditLimitType, value); }
        }

        public bool EnableCpoTransaction
        {
            get { return GetValue(() => EnableCpoTransaction); }
            set { SetValue(() => EnableCpoTransaction, value); }
        }
        public bool EnableItemBorrowTransaction
        {
            get { return GetValue(() => EnableItemBorrowTransaction); }
            set { SetValue(() => EnableItemBorrowTransaction, value); }
        } 
        #endregion

        #region Advanced Settings
        public bool HandleBankTransaction
        {
            get { return GetValue(() => HandleBankTransaction); }
            set { SetValue(() => HandleBankTransaction, value); }
        }
        public bool EnableReservations
        {
            get { return GetValue(() => EnableReservations); }
            set { SetValue(() => EnableReservations, value); }
        }

        public bool EnableCheckEntry
        {
            get { return GetValue(() => EnableCheckEntry); }
            set { SetValue(() => EnableCheckEntry, value); }
        }
        public bool PostWithLessStock
        {
            get { return GetValue(() => PostWithLessStock); }
            set { SetValue(() => PostWithLessStock, value); }
        } 
        #endregion

        #region Tax Settings
        public TaxTypes TaxType
        {
            get { return GetValue(() => TaxType); }
            set { SetValue(() => TaxType, value); }
        }
        [Range(0,100)]
        public decimal TaxPercent
        {
            get { return GetValue(() => TaxPercent); }
            set { SetValue(() => TaxPercent, value); }
        }
        public bool ByDefaultItemsHaveThisTaxRate
        {
            get { return GetValue(() => ByDefaultItemsHaveThisTaxRate); }
            set { SetValue(() => ByDefaultItemsHaveThisTaxRate, value); }
        }
        public bool ItemPricesAreTaxInclusive
        {
            get { return GetValue(() => ItemPricesAreTaxInclusive); }
            set { SetValue(() => ItemPricesAreTaxInclusive, value); }
        } 
        #endregion
        
        #region Sync Status
        public DateTime? LastToServerSyncDate
        {
            get { return GetValue(() => LastToServerSyncDate); }
            set { SetValue(() => LastToServerSyncDate, value); }
        }
        public DateTime? LastFromServerSyncDate
        {
            get { return GetValue(() => LastFromServerSyncDate); }
            set { SetValue(() => LastFromServerSyncDate, value); }
        }  
        #endregion

        [ForeignKey("Warehouse")]
        public int? WarehouseId { get; set; }
        public WarehouseDTO Warehouse
        {
            get { return GetValue(() => Warehouse); }
            set { SetValue(() => Warehouse, value); }
        }
    }

}
