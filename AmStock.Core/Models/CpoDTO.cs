using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMStock.Core.Models
{
    public class CpoDTO : CommonFieldsA
    {
        [Required]
        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        [Index(IsUnique = true)]
        public string Number
        {
            get { return GetValue(() => Number); }
            set { SetValue(() => Number, value); }
        }
        [Required]
        public DateTime PreparedDate
        {
            get { return GetValue(() => PreparedDate); }
            set { SetValue(() => PreparedDate, value); }
        }
        [NotMapped]
        public string PreparedDateString
        {
            get
            {
                return PreparedDate.ToString("dd-MM-yyyy") + "(" +
                       ReportUtility.getEthCalendarFormated(PreparedDate, "/") + ")";
            }
            set { SetValue(() => PreparedDateString, value); }
        }
        [Required]
        [MaxLength(255, ErrorMessage = "Exceeded 255 letters")]
        public string ToCompany
        {
            get { return GetValue(() => ToCompany); }
            set { SetValue(() => ToCompany, value); }
        }
        [Required]
        [Range(1, 1000000)]
        public decimal? Amount
        {
            get { return GetValue(() => Amount); }
            set { SetValue(() => Amount, value); }
        }

        [Required]
        public bool IsReturned
        {
            get { return GetValue(() => IsReturned); }
            set { SetValue(() => IsReturned, value); }
        }
        [NotMapped]
        public string IsReturnedString
        {
            get
            {
                return IsReturned ? "Yes" : "No";
            }
            set { SetValue(() => IsReturnedString, value); }
        }
        [NotMapped]
        public string AmountString
        {
            get
            {
                return Amount.ToString();
            }
            set { SetValue(() => AmountString, value); }
        }
        public DateTime? ReturnedDate
        {
            get { return GetValue(() => ReturnedDate); }
            set { SetValue(() => ReturnedDate, value); }
        }
        [NotMapped]
        public string ReturnDateString
        {
            get
            {
                return ReturnedDate != null ? ReturnedDate.Value.ToString("dd-MM-yyyy") : DateTime.Now.ToString("dd-MM-yyyy");
            }
            set { SetValue(() => ReturnDateString, value); }
        }

        [ForeignKey("Warehouse")]
        public int WarehouseId { get; set; }
        public WarehouseDTO Warehouse
        {
            get { return GetValue(() => Warehouse); }
            set { SetValue(() => Warehouse, value); }
        }
    }
}
