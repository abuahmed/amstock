using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AMStock.Core.Enumerations;
using AMStock.Core.Extensions;

namespace AMStock.Core.Models
{
    public class PaymentDTO : CommonFieldsA
    {
        [Required]
        public PaymentTypes PaymentType 
        {
            get { return GetValue(() => PaymentType); }
            set { SetValue(() => PaymentType, value); }
        }

        [Required]
        public DateTime PaymentDate
        {
            get { return GetValue(() => PaymentDate); }
            set { SetValue(() => PaymentDate, value); }
        }

        [Required]
        public string Reason
        {
            get { return GetValue(() => Reason); }
            set { SetValue(() => Reason, value); }
        }

        [Required]
        [MaxLength(50, ErrorMessage = "Exceeded 50 letters")]
        public string PersonName
        {
            get { return GetValue(() => PersonName); }
            set { SetValue(() => PersonName, value); }
        }

        public string PaymentRemark
        {
            get { return GetValue(() => PaymentRemark); }
            set { SetValue(() => PaymentRemark, value); }
        }

        [Required]
        public decimal Amount
        {
            get { return GetValue(() => Amount); }
            set { SetValue(() => Amount, value); }
        }

        public PaymentMethods PaymentMethod
        {
            get { return GetValue(() => PaymentMethod); }
            set { SetValue(() => PaymentMethod, value); }
        }

        public PaymentStatus Status
        {
            get { return GetValue(() => Status); }
            set { SetValue(() => Status, value); }
        }

        public DateTime? DueDate
        {
            get { return GetValue(() => DueDate); }
            set { SetValue(() => DueDate, value); }
        }
        
        [ForeignKey("Check")]
        public int? CheckId { get; set; }
        public CheckDTO Check
        {
            get { return GetValue(() => Check); }
            set { SetValue(() => Check, value); }
        }

        [ForeignKey("Clearance")]
        public int? ClearanceId { get; set; }
        public PaymentClearanceDTO Clearance
        {
            get { return GetValue(() => Clearance); }
            set { SetValue(() => Clearance, value); }
        }

        [ForeignKey("Transaction")]
        public int? TransactionId { get; set; }
        public TransactionHeaderDTO Transaction
        {
            get { return GetValue(() => Transaction); }
            set { SetValue(() => Transaction, value); }
        }
     
        [ForeignKey("Warehouse")]
        public int WarehouseId { get; set; }
        public WarehouseDTO Warehouse
        {
            get { return GetValue(() => Warehouse); }
            set { SetValue(() => Warehouse, value); }
        }
        
        [NotMapped]
        [DisplayName("Payment Date")]
        public string PaymentDateString
        {
            get
            {
                return PaymentDate.ToString("dd-MM-yyyy") + "(" + ReportUtility.getEthCalendarFormated(PaymentDate, "/") + ")";
            }
            set { SetValue(() => PaymentDateString, value); }
        }
        
        [NotMapped]
        [DisplayName("Amount")]
        public string AmountString
        {
            get { return Amount.ToString("C"); }
            set { SetValue(() => AmountString, value); }
        }
        
        [NotMapped]
        [DisplayName("Payment Type")]
        public string PaymentTypeString
        {
            get
            {
               return EnumUtil.GetEnumDesc(PaymentType);
            }
            set { SetValue(() => PaymentTypeString, value); }
        }
        
        [NotMapped]
        [DisplayName("Status")]
        public string StatusString
        {
            get
            {
                //return PaymentType.ToString();
                return EnumUtil.GetEnumDesc(Status);
            }
            set { SetValue(() => StatusString, value); }
        }
        
        [NotMapped]
        [DisplayName("Payment Method")]
        public string PaymentMethodString
        {
            get
            {
                //return PaymentType.ToString();
                return EnumUtil.GetEnumDesc(PaymentMethod);
            }
            set { SetValue(() => StatusString, value); }
        }
    }
}
