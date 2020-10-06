using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMStock.Core.Models
{
    public class CheckDTO : CommonFieldsA
    {
       [Required]
        public DateTime CheckDate
        {
            get { return GetValue(() => CheckDate); }
            set
            {
                SetValue(() => CheckDate, value);
                SetValue<string>(() => CheckDetail, value.ToString());
            }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        [Required]
        public string CheckNumber
        {
            get { return GetValue(() => CheckNumber); }
            set
            {
                SetValue(() => CheckNumber, value);
                SetValue(() => CheckDetail, value);
            }
        }

        [ForeignKey("CustomerBankAccount")]
        public int CustomerBankAccountId { get; set; }
        public FinancialAccountDTO CustomerBankAccount
        {
            get { return GetValue(() => CustomerBankAccount); }
            set { SetValue(() => CustomerBankAccount, value); }
        }

        [ForeignKey("ClientBankAccount")]
        public int? ClientBankAccountId { get; set; }
        public FinancialAccountDTO ClientBankAccount
        {
            get { return GetValue(() => ClientBankAccount); }
            set { SetValue(() => ClientBankAccount, value); }
        }

        [NotMapped]
        public string CheckAmount
        {
            get { return GetValue(() => CheckAmount); }
            set { SetValue(() => CheckAmount, value); }
        }
        [NotMapped]
        public DateTime CheckDueDate
        {
            get { return GetValue(() => CheckDueDate); }
            set { SetValue(() => CheckDueDate, value); }
        }
        [NotMapped]
        public string CheckDetail
        {
            get
            {
                return CheckNumber + " - " + 
                    CheckDate.ToString("dd-MM-yyyy") + "(" + 
                    ReportUtility.getEthCalendarFormated(CheckDate, "/") + ")";
            }
            set { SetValue(() => CheckDetail, value); }
        }
    }
}