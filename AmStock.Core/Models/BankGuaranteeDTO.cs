using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMStock.Core.Models
{
    public class BankGuaranteeDTO : CommonFieldsA
    {
        [Required]
        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        public string BankName
        {
            get { return GetValue(() => BankName); }
            set { SetValue(() => BankName, value); }
        }
        
        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        public string BankBranch
        {
            get { return GetValue(() => BankBranch); }
            set { SetValue(() => BankBranch, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        public string PropertyType
        {
            get { return GetValue(() => PropertyType); }
            set { SetValue(() => PropertyType, value); }
        }
        
        public string PropertyDescription
        {
            get { return GetValue(() => PropertyDescription); }
            set { SetValue(() => PropertyDescription, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        public string AccountNumber
        {
            get { return GetValue(() => AccountNumber); }
            set { SetValue(() => AccountNumber, value); }
        }

        [Required]
        [Range(1,1000000000)] 
        public decimal? GuaranteedAmount
        {
            get { return GetValue(() => GuaranteedAmount); }
            set { SetValue(() => GuaranteedAmount, value); }
        }
        
        [ForeignKey("Warehouse")]
        public int? WarehouseId { get; set; }
        public WarehouseDTO Warehouse
        {
            get { return GetValue(() => Warehouse); }
            set { SetValue(() => Warehouse, value); }
        }
    }
}