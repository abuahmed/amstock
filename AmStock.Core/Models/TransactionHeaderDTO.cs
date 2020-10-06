using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Mapping;
using System.Linq;
using System.Net;
using AMStock.Core.Enumerations;
using AMStock.Core.Extensions;

namespace AMStock.Core.Models
{
    public partial class TransactionHeaderDTO : CommonFieldsA
    {
        public TransactionHeaderDTO()
        {
            TransactionLines = new List<TransactionLineDTO>();
            Payments = new List<PaymentDTO>();
        }

        public TransactionTypes TransactionType
        {
            get { return GetValue(() => TransactionType); }
            set { SetValue(() => TransactionType, value); }
        }

        [DisplayName("Transaction No.")]
        [MaxLength(255, ErrorMessage = "Exceeded 255 letters")]
        public string TransactionNumber
        {
            get { return GetValue(() => TransactionNumber); }
            set { SetValue(() => TransactionNumber, value); }
        }

        [RegularExpression("^[0-9]{8}$", ErrorMessage = "Invalid Fiscal No, Must be 10 digit")]
        [MaxLength(255, ErrorMessage = "Exceeded 255 letters")]
        public string FiscalNumber
        {
            get { return GetValue(() => FiscalNumber); }
            set { SetValue(() => FiscalNumber, value); }
        }

        [DisplayName("On Date")]
        public DateTime TransactionDate
        {
            get { return GetValue(() => TransactionDate); }
            set { SetValue(() => TransactionDate, value); }
        }

        public TransactionStatus Status
        {
            get { return GetValue(() => Status); }
            set
            {
                SetValue(() => Status, value);
                SetValue<string>(() => StatusString, value.ToString());
                SetValue<string>(() => PaymentCompleted, value.ToString());
            }
        }

        [ForeignKey("Warehouse")]
        public int WarehouseId { get; set; }
        public WarehouseDTO Warehouse
        {
            get { return GetValue(() => Warehouse); }
            set { SetValue(() => Warehouse, value); }
        }

        [ForeignKey("BusinessPartner")]
        public int BusinessPartnerId { get; set; }
        public BusinessPartnerDTO BusinessPartner
        {
            get { return GetValue(() => BusinessPartner); }
            set { SetValue(() => BusinessPartner, value); }
        }
        
        public ICollection<TransactionLineDTO> TransactionLines
        {
            get { return GetValue(() => TransactionLines); }
            set
            {
                SetValue(() => TransactionLines, value);
            }
        }
        public ICollection<PaymentDTO> Payments
        {
            get { return GetValue(() => Payments); }
            set
            {
                SetValue(() => Payments, value);
                SetValue(() => PaymentCompleted, value.ToString());
            }
        }
    }

    public partial class TransactionHeaderDTO
    {
        public DateTime? OrderDate
        {
            get { return GetValue(() => OrderDate); }
            set { SetValue(() => OrderDate, value); }
        }

        public DateTime? RequiredDate
        {
            get { return GetValue(() => RequiredDate); }
            set { SetValue(() => RequiredDate, value); }
        }

        public DateTime? ShippedDate
        {
            get { return GetValue(() => ShippedDate); }
            set { SetValue(() => ShippedDate, value); }
        }

        [DisplayName("No. of Items")]
        public int NoofItems
        {
            get { return GetValue(() => NoofItems); }
            set { SetValue(() => NoofItems, value); }
        }

        [DisplayName("Sub Total")]
        public decimal SubTotal
        {
            get { return GetValue(() => SubTotal); }
            set { SetValue(() => SubTotal, value); }
        }

        [DisplayName("Tax Amount")]
        public decimal TaxAmt
        {
            get { return GetValue(() => TaxAmt); }
            set { SetValue(() => TaxAmt, value); }
        }

        [DisplayName("Freight")]
        public decimal Freight
        {
            get { return GetValue(() => Freight); }
            set { SetValue(() => Freight, value); }
        }

        [DisplayName("Total Due")]
        public decimal TotalDue
        {
            get { return GetValue(() => TotalDue); }
            set { SetValue(() => TotalDue, value); }
        }

        [DisplayName("Total Payment Cleared")]
        public decimal TotallyCleared
        {
            get { return GetValue(() => TotallyCleared); }
            set { SetValue(() => TotallyCleared, value); }
        }

        [DisplayName("Total Purchasing Cost")]
        public decimal TotalPurchasingCost
        {
            get { return GetValue(() => TotalPurchasingCost); }
            set { SetValue(() => TotalPurchasingCost, value); }
        }

        [DisplayName("Comment")]
        public string Comment
        {
            get { return GetValue(() => Comment); }
            set { SetValue(() => Comment, value); }
        }

        [DisplayName("Payment Current Status")]
        public string PaymentCurrentStatus
        {
            get { return GetValue(() => PaymentCurrentStatus); }
            set { SetValue(() => PaymentCurrentStatus, value); }
        }
    }

    public partial class TransactionHeaderDTO
    {
        [NotMapped]
        [DisplayName("No. of Items")]
        public int CountLines
        {
            get { return TransactionLines.Count(l => l.Enabled); }
            set { SetValue(() => CountLines, value); }
        }
        [NotMapped]
        [DisplayName("Total Cost")]
        public decimal TotalCost
        {
            get { return TransactionLines.Where(l => l.Enabled).Sum(l => l.LinePrice); }
            set
            {
                SetValue(() => TotalCost, value);
                SetValue(() => TotalCostString, value.ToString());
            }
        }
        [NotMapped]
        [DisplayName("Total Paid")]
        public string TotalPaid
        {
            get
            {
                return Payments.Where(l => l.Enabled).Sum(p => p.Amount).ToString("N");
            }
            set { SetValue(() => TotalPaid, value); }
        }
        [NotMapped]
        [DisplayName("Amount Left")]
        public decimal AmountLeft
        {
            get
            {
                return Status != TransactionStatus.Order ?
                    Payments.Where(l => l.Enabled)
                    .Where(p => p.Status == PaymentStatus.NotCleared && p.PaymentMethod != PaymentMethods.Cash)
                    .Sum(p => p.Amount) : 0;
            }
            set { SetValue(() => AmountLeft, value); }
        }
        [NotMapped]
        public decimal TotalPurchaseCost
        {
            get { return TransactionLines.Where(l => l.Enabled).Sum(l => l.LinePurchasePrice); }
            set { SetValue(() => TotalCost, value); }
        }
        [NotMapped]
        public bool IsPaymentCompleted
        {
            get
            {
                return TotalCost == Payments.Sum(p => p.Amount);
            }
            set { SetValue(() => IsPaymentCompleted, value); }
        }
        [NotMapped]
        [DisplayName("Payment Status")]
        public string PaymentCompleted
        {
            get
            {
                if (!Payments.Any(l => l.Enabled))
                    return EnumUtil.GetEnumDesc(PaymentStatus.NoPayment);

                var payment =
                    Payments.Where(l => l.Enabled).FirstOrDefault(
                        p => p.Status == PaymentStatus.NotCleared || p.Status == PaymentStatus.NotDeposited);

                return payment != null ?
                    EnumUtil.GetEnumDesc(PaymentStatus.NotCleared) : EnumUtil.GetEnumDesc(PaymentStatus.Cleared);
            }
            set { SetValue(() => PaymentCompleted, value); }
        }
        [NotMapped]
        [DisplayName("No. of Payments Made")]
        public string NoOfPaymentsMade
        {
            get { return Payments.Count(l => l.Enabled).ToString(); }
            set { SetValue(() => NoOfPaymentsMade, value); }
        }
        [NotMapped]
        [DisplayName("On Date")]
        public string TransactionDateString
        {
            get
            {
                return TransactionDate.ToString("dd-MM-yyyy");
            }
            set { SetValue(() => TransactionDateString, value); }
        }
        [NotMapped]
        [DisplayName("On Date")]
        public string TransactionDateStringAmharic
        {
            get
            {
                return TransactionDate.ToString("dd-MM-yyyy") +"(" + ReportUtility.getEthCalendarFormated(TransactionDate,"-")+")" ;
            }
            set { SetValue(() => TransactionDateStringAmharic, value); }
        }
        [NotMapped]
        [DisplayName("Transaction Status")]
        public string StatusString
        {
            get
            {
                return EnumUtil.GetEnumDesc(Status);
            }
            set { SetValue(() => StatusString, value); }
        }
        [NotMapped]
        [DisplayName("Amount Left")]
        public string AmountLeftString
        {
            get
            {
                return AmountLeft.ToString("N");
            }
            set { SetValue(() => AmountLeftString, value); }
        }
        [NotMapped]
        [DisplayName("Total Cost")]
        public string TotalCostString
        {
            get
            {
                return TotalCost.ToString("C");
            }
            set { SetValue(() => TotalCostString, value); }
        }
    }
}
