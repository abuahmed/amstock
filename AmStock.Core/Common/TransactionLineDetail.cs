using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using AMStock.Core.Models;

namespace AMStock.Core.Common
{
    public class TransactionLineHeader : CommonFieldsA
    {
        public TransactionLineHeader()
        {
            TransactionLineDetails = new List<TransactionLineDetail>();
        }

        public int TransactionType
        {
            get { return GetValue(() => TransactionType); }
            set { SetValue(() => TransactionType, value); }
        }

        public IEnumerable<TransactionLineDetail> TransactionLineDetails
        {
            get { return GetValue(() => TransactionLineDetails); }
            set { SetValue(() => TransactionLineDetails, value); }
        }
    }

    public class TransactionLineDetail : CommonFieldsA
    {
        #region Transaction Header
     
        public int TransactionId
        {
            get { return GetValue(() => TransactionId); }
            set { SetValue(() => TransactionId, value); }
        }
        
        [DisplayName("Transaction No.")]
        public string TransactionNumber
        {
            get { return GetValue(() => TransactionNumber); }
            set { SetValue(() => TransactionNumber, value); }
        }
        public string TransactionGuid
        {
            get { return GetValue(() => TransactionGuid); }
            set { SetValue(() => TransactionGuid, value); }
        }
        [DisplayName("On Date")]
        public DateTime TransactionDate
        {
            get { return GetValue(() => TransactionDate); }
            set { SetValue(() => TransactionDate, value); }
        }
        [DisplayName("On Date")]
        public string TransactionDateString
        {
            get
            {
                return TransactionDate.ToString("dd-MM-yyyy") + "(" +
                       ReportUtility.getEthCalendarFormated(TransactionDate, "/") + ")";
                //return GetValue(() => TransactionDateString);
            }
            set { SetValue(() => TransactionDateString, value); }
        }
       
        public string TransactionStatus
        {
            get { return GetValue(() => TransactionStatus); }
            set { SetValue(() => TransactionStatus, value); }
        }
        [DisplayName("Name")]
        public string DisplayName
        {
            get { return GetValue(() => DisplayName); }
            set { SetValue(() => DisplayName, value); }
        }
        public int BusinessPartnerId
        {
            get { return GetValue(() => BusinessPartnerId); }
            set { SetValue(() => BusinessPartnerId, value); }
        }
        public int CountLines
        {
            get { return GetValue(() => CountLines); }
            set { SetValue(() => CountLines, value); }
        }
        public decimal TotalCost
        {
            get { return GetValue(() => TotalCost); }
            set { SetValue(() => TotalCost, value); }
        }
        public string TotalPaid
        {
            get { return GetValue(() => TotalPaid); }
            set { SetValue(() => TotalPaid, value); }
        }
        public string AmountLeft
        {
            get { return GetValue(() => AmountLeft); }
            set { SetValue(() => AmountLeft, value); }
        }
        public string NoOfPaymentsMade
        {
            get { return GetValue(() => NoOfPaymentsMade); }
            set { SetValue(() => NoOfPaymentsMade, value); }
        } 
        #endregion

        #region Transaction Lines
        [DisplayName("Item Code")]
        public string ItemCode
        {
            get { return GetValue(() => ItemCode); }
            set { SetValue(() => ItemCode, value); }
        }
        [DisplayName("Item Name")]
        public string ItemDisplayName
        {
            get { return GetValue(() => ItemDisplayName); }
            set { SetValue(() => ItemDisplayName, value); }
        }
        public decimal Unit
        {
            get { return GetValue(() => Unit); }
            set { SetValue(() => Unit, value); }
        }
        [DisplayName("Each Price")]
        public decimal EachPrice
        {
            get { return GetValue(() => EachPrice); }
            set { SetValue(() => EachPrice, value); }
        }
        [DisplayName("Line Price")]
        public decimal LinePrice
        {
            get { return GetValue(() => LinePrice); }
            set { SetValue(() => LinePrice, value); }
        }
        public decimal PurchasePrice
        {
            get { return GetValue(() => PurchasePrice); }
            set { SetValue(() => PurchasePrice, value); }
        } 
        #endregion

        #region Payment Lines
        public DateTime DueDate
        {
            get { return GetValue(() => DueDate); }
            set { SetValue(() => DueDate, value); }
        }
        public string DueDateString
        {
            get { return DueDate.ToString("dd-MM-yyyy"); }
            set { SetValue(() => DueDateString, value); }
        }
        public string PaymentMethod
        {
            get { return GetValue(() => PaymentMethod); }
            set { SetValue(() => PaymentMethod, value); }
        }
        public decimal PaymentAmount
        {
            get { return GetValue(() => PaymentAmount); }
            set { SetValue(() => PaymentAmount, value); }
        }
        public int PaymentId
        {
            get { return GetValue(() => PaymentId); }
            set { SetValue(() => PaymentId, value); }
        }
        #endregion
        [DisplayName("Store")]
        public string WarehouseName
        {
            get { return GetValue(() => WarehouseName); }
            set { SetValue(() => WarehouseName, value); }
        }
    }
}
