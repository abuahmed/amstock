using System;
using AMStock.Core;
using AMStock.Core.Models;

namespace AMStock.WPF.Models
{
    public class TransactionHeaderModel : EntityBase
    {
        #region Transaction Header

        public int TransactionId
        {
            get { return GetValue(() => TransactionId); }
            set { SetValue(() => TransactionId, value); }
        }
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
        public DateTime TransactionDate
        {
            get { return GetValue(() => TransactionDate); }
            set { SetValue(() => TransactionDate, value); }
        }
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

        public string DisplayName
        {
            get { return GetValue(() => DisplayName); }
            set { SetValue(() => DisplayName, value); }
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

        public string ItemCode
        {
            get { return GetValue(() => ItemCode); }
            set { SetValue(() => ItemCode, value); }
        }
        public string ItemDisplayName
        {
            get { return GetValue(() => ItemDisplayName); }
            set { SetValue(() => ItemDisplayName, value); }
        }

        public int Unit
        {
            get { return GetValue(() => Unit); }
            set { SetValue(() => Unit, value); }
        }
        public decimal EachPrice
        {
            get { return GetValue(() => EachPrice); }
            set { SetValue(() => EachPrice, value); }
        }
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
    }
}