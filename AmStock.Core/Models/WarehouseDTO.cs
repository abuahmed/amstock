using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMStock.Core.Models
{
    public class WarehouseDTO : CommonFieldsE
    {
        public WarehouseDTO() 
        {
            FinancialAccounts = new HashSet<FinancialAccountDTO>();
            Transactions = new List<TransactionHeaderDTO>();
            PhysicalInventories = new List<PhysicalInventoryHeaderDTO>();
        }

        [Key]
        [Column(Order = 1)]
        public int WarehouseNumber
        {
            get { return GetValue(() => WarehouseNumber); }
            set { SetValue(() => WarehouseNumber, value); }
        }

        [ForeignKey("Organization")]
        public int OrganizationId { get; set; }
        public OrganizationDTO Organization
        {
            get { return GetValue(() => Organization); }
            set { SetValue(() => Organization, value); }
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
        
        public bool IsDefault//also "IS HeadQuarter"
        {
            get { return GetValue(() => IsDefault); }
            set { SetValue(() => IsDefault, value); }
        }
        
        [NotMapped]
        public string IsDefaultYesNo {
            get
            {
                return IsDefault ? "Yes" : "No";
            }
        }
        
        [NotMapped]
        public bool DefaultCheckBoxEnability { 
            get 
            {
                return !IsDefault; 
            } 
        }
        
        [NotMapped]
        public string DisplayNameShort
        {
            get { return DisplayName != null && DisplayName.Length > 18 ? DisplayName.Substring(0, 15) + "..." : DisplayName; }
            set { SetValue(() => DisplayNameShort, value); }
        }
        
        public ICollection<FinancialAccountDTO> FinancialAccounts
        {
            get { return GetValue(() => FinancialAccounts); }
            set { SetValue(() => FinancialAccounts, value); }
        }
        public ICollection<TransactionHeaderDTO> Transactions
        {
            get { return GetValue(() => Transactions); }
            set { SetValue(() => Transactions, value); }
        }
        public ICollection<PhysicalInventoryHeaderDTO> PhysicalInventories
        {
            get { return GetValue(() => PhysicalInventories); }
            set { SetValue(() => PhysicalInventories, value); }
        }
    }
}
