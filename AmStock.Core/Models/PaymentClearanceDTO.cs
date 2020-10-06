using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AMStock.Core.CustomValidationAttributes;

namespace AMStock.Core.Models
{
    public class PaymentClearanceDTO : CommonFieldsA
    {
        [MaxLength(50, ErrorMessage = "Statement Number exceeded 50 letters")]
        [ExcludeChar("/.,!@#$%", ErrorMessage = "Statement Number contains invalid letters")]
        public string StatementNumber
        {
            get { return GetValue(() => StatementNumber); }
            set { SetValue(() => StatementNumber, value); }
        }

        public DateTime? StatementDate
        {
            get { return GetValue(() => StatementDate); }
            set { SetValue(() => StatementDate, value); }
        }

        public DateTime? DepositDate
        {
            get { return GetValue(() => DepositDate); }
            set { SetValue(() => DepositDate, value); }
        }

        //[ForeignKey("Payment")]
        //public int PaymentId { get; set; }
        //public PaymentDTO Payment
        //{
        //    get { return GetValue(() => Payment); }
        //    set { SetValue(() => Payment, value); }
        //}

        [ForeignKey("ClientAccount")]
        public int ClientAccountId { get; set; }
        public FinancialAccountDTO ClientAccount
        {
            get { return GetValue(() => ClientAccount); }
            set { SetValue(() => ClientAccount, value); }
        }

        //[ForeignKey("DepositedBy")]
        //public int? DepositedById { get; set; }
        //public UserDTO DepositedBy
        //{
        //    get { return GetValue(() => DepositedBy); }
        //    set { SetValue(() => DepositedBy, value); }
        //}
        public DateTime? DepositedOnDate
        {
            get { return GetValue(() => DepositedOnDate); }
            set { SetValue(() => DepositedOnDate, value); }
        }
        [NotMapped]
        public string DepositDateString
        {
            get
            {
                return DepositedOnDate != null
                    ? DepositedOnDate.Value.ToString("dd-MM-yyyy") + "(" +
                      ReportUtility.getEthCalendarFormated(DepositedOnDate.Value, "/") + ")"
                    : "";
            }
            set { SetValue(() => DepositDateString, value); }
        }
        //[NotMapped]
        //public string DepositedUserString
        //{
        //    get
        //    {
        //        return DepositedBy != null ? DepositedBy.UserName : "";
        //    }
        //    set { SetValue(() => DepositedUserString, value); }
        //}

        //[ForeignKey("ClearedBy")]
        //public int? ClearedById { get; set; }
        //public UserDTO ClearedBy
        //{
        //    get { return GetValue(() => ClearedBy); }
        //    set { SetValue(() => ClearedBy, value); }
        //}
        public DateTime? ClearedOnDate
        {
            get { return GetValue(() => ClearedOnDate); }
            set { SetValue(() => ClearedOnDate, value); }
        }
        [NotMapped]
        public string ClearedDateString
        {
            get
            {
                return ClearedOnDate != null
                    ? ClearedOnDate.Value.ToString("dd-MM-yyyy") + "(" +
                      ReportUtility.getEthCalendarFormated(ClearedOnDate.Value, "/") + ")"
                    : "";
            }
            set { SetValue(() => ClearedDateString, value); }
        }
        //[NotMapped]
        //public string ClearedUserString
        //{
        //    get
        //    {
        //        return ClearedBy != null ? ClearedBy.UserName : "";
        //    }
        //    set { SetValue(() => ClearedUserString, value); }
        //}
    }
}