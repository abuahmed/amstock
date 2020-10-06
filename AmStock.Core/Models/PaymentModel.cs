using System;
using System.ComponentModel.DataAnnotations;

namespace AMStock.Core.Models
{
    public class PaymentModel : EntityBase
    {
        public decimal AmountRequired
        {
            get { return GetValue(() => AmountRequired); }
            set
            {
                SetValue(() => AmountRequired, value);
                SetValue(() => Change, value);
                SetValue(() => AmountLeft, value);
                SetValue<string>(() => CreditVisibility, value.ToString());
            }
        }
        [Required]
        public DateTime PaymentDate
        {
            get { return GetValue(() => PaymentDate); }
            set { SetValue(() => PaymentDate, value); }
        }
        [Required]
        [Range(0, 1000000)]
        public decimal Amount
        {
            get { return GetValue(() => Amount); }
            set
            {
                SetValue(() => Amount, value);
                SetValue(() => Change, value);
                SetValue(() => AmountLeft, value);
                SetValue<string>(() => CreditVisibility, value.ToString());
            }
        }
        public decimal AmountLeft
        {
            get
            {
                return (AmountRequired - Amount) < 0 ? 0 : AmountRequired - Amount;
            }
            set
            {
                SetValue(() => AmountLeft, value);
            }
        }
        public decimal Change
        {
            get
            {
                return Amount - AmountRequired < 0 ? 0 : Amount - AmountRequired;
            }
            set { SetValue(() => Change, value); }
        }

        public string CreditVisibility
        {
            get
            {
                return !ReachedLimit && AmountLeft > 0 ? "Visible" : "Collapsed";
            }
            set { SetValue(() => CreditVisibility, value); }
        }
        public bool ReachedLimit
        {
            get
            {
                return GetValue(() => ReachedLimit);
            }
            set { SetValue(() => ReachedLimit, value); }
        }
    }
}