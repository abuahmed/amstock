using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMStock.Core.Models
{
    public class FinancialAccountDTO : CommonFieldsA
    {
        [Required]
        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        public string BankName
        {
            get { return GetValue(() => BankName); }
            set { SetValue(() => BankName, value); }
        }

        [Required]
        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        public string BankBranch
        {
            get { return GetValue(() => BankBranch); }
            set { SetValue(() => BankBranch, value); }
        }

        [Required]
        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        public string AccountNumber
        {
            get { return GetValue(() => AccountNumber); }
            set { SetValue(() => AccountNumber, value); }
        }

        [NotMapped]
        public string AccountDetail
        {
            get { return BankName + "(" + AccountNumber + ")"; }
            set { SetValue(() => AccountDetail, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        public string AccountFormat
        {
            get { return GetValue(() => AccountFormat); }
            set { SetValue(() => AccountFormat, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        public string Iban
        {
            get { return GetValue(() => Iban); }
            set { SetValue(() => Iban, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        public string SwiftCode
        {
            get { return GetValue(() => SwiftCode); }
            set { SetValue(() => SwiftCode, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        public string Country
        {
            get { return GetValue(() => Country); }
            set { SetValue(() => Country, value); }
        }

        [ForeignKey("Warehouse")]
        public int? WarehouseId { get; set; }
        public WarehouseDTO Warehouse
        {
            get { return GetValue(() => Warehouse); }
            set { SetValue(() => Warehouse, value); }
        }

        [ForeignKey("BusinessPartner")]
        public int? BusinessPartnerId { get; set; }
        public BusinessPartnerDTO BusinessPartner
        {
            get { return GetValue(() => BusinessPartner); }
            set { SetValue(() => BusinessPartner, value); }
        }
    }
}
