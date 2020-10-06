using System;
using System.ComponentModel.DataAnnotations;
using AMStock.Core.CustomValidationAttributes;

namespace AMStock.Core.Models
{
    public class PaymentTransaction : CommonFieldsA
    {
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
        [GreaterThanZero(ErrorMessage = "Can't be less than or equal to 0")]
        public double Amount
        {
            get { return GetValue(() => Amount); }
            set { SetValue(() => Amount, value); }
        }
        public int? PaymentMethod
        {
            get { return GetValue(() => PaymentMethod); }
            set { SetValue(() => PaymentMethod, value); }
        }


    }
}
