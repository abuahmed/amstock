using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using AMStock.Core.CustomValidationAttributes;
using AMStock.Core.Enumerations;
using AMStock.Core.Extensions;

namespace AMStock.Core.Models
{
    public partial class BusinessPartnerDTO : CommonFieldsB
    {
        public BusinessPartnerDTO()
        {
            TransactionHeaders = new List<TransactionHeaderDTO>();
            FinancialAccounts = new HashSet<FinancialAccountDTO>();
            Addresses = new HashSet<BusinessPartnerAddressDTO>();
            Contacts = new HashSet<BusinessPartnerContactDTO>();
        }

        public BusinessPartnerTypes BusinessPartnerType
        {
            get { return GetValue(() => BusinessPartnerType); }
            set { SetValue(() => BusinessPartnerType, value); }
        }

        public BusinessPartnerCategory Category
        {
            get { return GetValue(() => Category); }
            set { SetValue(() => Category, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        [DisplayName("TIN Number")]
        [RegularExpression("^[0-9]{10}$", ErrorMessage = "Invalid Tin No, Must be 10 digit")]
        public string TinNumber
        {
            get { return GetValue(() => TinNumber); }
            set { SetValue(() => TinNumber, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        [DisplayName("VAT Number")]
        [RegularExpression("^[0-9]{10}$", ErrorMessage = "Invalid VAT No, Must be 10 digit")]
        public string VatNumber
        {
            get { return GetValue(() => VatNumber); }
            set { SetValue(() => VatNumber, value); }
        }
        
        [MaxLength(30, ErrorMessage = "Code exceeded 50 letters")]
        [ExcludeChar("/.,!@#$%", ErrorMessage = "Code contains invalid letters")]
        public string Code
        {
            get { return GetValue(() => Code); }
            set { SetValue(() => Code, value); }
        }

        [Display(Name = "Credit Limit")]
        public decimal CreditLimit
        {
            get
            {
                //if (PaymentMethod == PaymentMethods.Cash)
                //return 0;
                return GetValue(() => CreditLimit);
            }
            set { SetValue(() => CreditLimit, value); }
        }

        [Display(Name = "Max No Of Credit Transactions")]
        public int MaxNoCreditTransactions
        {
            get
            {
                //if (PaymentMethod == PaymentMethods.Cash)
                //return 0;
                return GetValue(() => MaxNoCreditTransactions);
            }
            set { SetValue(() => MaxNoCreditTransactions, value); }
        }

        [Display(Name = "Payment Term")]
        public int PaymentTerm
        {
            get
            {
                //if (PaymentMethod == PaymentMethods.Cash)
                //return 0;
                return GetValue(() => PaymentTerm);
            }
            set { SetValue(() => PaymentTerm, value); }
        }

        [Display(Name = "Allow Credits Without Check")]
        public bool AllowCreditsWithoutCheck
        {
            get
            {
                return GetValue(() => AllowCreditsWithoutCheck);
            }
            set { SetValue(() => AllowCreditsWithoutCheck, value); }
        }

        public decimal TotalCredits
        {
            get { return GetValue(() => TotalCredits); }
            set { SetValue(() => TotalCredits, value); }
        }

        public int TotalNoOfCreditTransactions
        {
            get { return GetValue(() => TotalNoOfCreditTransactions); }
            set { SetValue(() => TotalNoOfCreditTransactions, value); }
        }

        //[ForeignKey("SalesPerson")]
        //public int? SalesPersonId { get; set; }
        //public SalesPersonDTO SalesPerson
        //{
        //    get { return GetValue(() => SalesPerson); }
        //    set { SetValue(() => SalesPerson, value); }
        //}

        public ICollection<TransactionHeaderDTO> TransactionHeaders
        {
            get { return GetValue(() => TransactionHeaders); }
            set { SetValue(() => TransactionHeaders, value); }
        }
        public ICollection<FinancialAccountDTO> FinancialAccounts
        {
            get { return GetValue(() => FinancialAccounts); }
            set { SetValue(() => FinancialAccounts, value); }
        }
        public ICollection<BusinessPartnerAddressDTO> Addresses
        {
            get { return GetValue(() => Addresses); }
            set { SetValue(() => Addresses, value); }
        }
        public ICollection<BusinessPartnerContactDTO> Contacts
        {
            get { return GetValue(() => Contacts); }
            set { SetValue(() => Contacts, value); }
        }
    }

    public partial class BusinessPartnerDTO
    {
        [NotMapped]
        [Display(Name = "Total Credits")]
        public decimal TotalCredit
        {
            get
            {
                try
                {
                    return TransactionHeaders.Where(l => l.Enabled).Sum(p => p.AmountLeft);
                }
                catch
                {
                    return 0;
                }
            }
            set { SetValue(() => TotalCredit, value); }
        }

        [NotMapped]
        [Display(Name = "Total Credits")]
        public string TotalCreditString
        {
            get
            {
                return TotalCredits.ToString("C");
            }
            set { SetValue(() => TotalCreditString, value); }
        }

        [NotMapped]
        [Display(Name = "Total No Of Outstanding Transactions")]
        public int TotalNoOfOutstandingTransactions
        {
            get
            {
                try
                {
                    return TransactionHeaders.Where(l => l.Enabled).Count(s => s.PaymentCompleted == EnumUtil.GetEnumDesc(PaymentStatus.NotCleared));
                }
                catch
                {
                    return 0;
                }
            }
            set { SetValue(() => TotalNoOfOutstandingTransactions, value); }
        }

        [NotMapped]
        public string DisplayNameShort
        {
            get
            {
                return DisplayName != null && DisplayName.Length > 18 ? DisplayName.Substring(0, 15) + "..." : DisplayName;
            }
            set { SetValue(() => DisplayNameShort, value); }
        }
    }
}
