using AMStock.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace AMStock.WPF.Models
{
    public class TransactionLineModel : EntityBase
    {
        public ItemQuantityDTO ItemQuantity
        {
            get { return GetValue(() => ItemQuantity); }
            set { SetValue(() => ItemQuantity, value); }
        }

        [Required]
        [Range(1, 10000000)]
        public decimal? UnitQuantity
        {
            get { return GetValue(() => UnitQuantity); }
            set
            {
                SetValue(() => UnitQuantity, value);
                SetValue(() => LineTotal, (decimal)value);
                //SetValue(() => Difference, value);
            }
        }
        [Required]
        [Range(1, 10000000)]
        public decimal EachPrice
        {
            get
            {
                return GetValue(() => EachPrice);
//Convert.ToDecimal(
//                     (ItemQuantity.Item.SellPrice / 
//                     (1 + (Singleton.Setting.TaxPercent * (decimal)0.01))).ToString("N2")
//                     )
                //return ItemQuantity != null && ItemQuantity.Item != null ?
                //     ItemQuantity.Item.SellPrice
                //    : 0;
            }
            set
            {
                SetValue(() => EachPrice, value);
                SetValue(() => LineTotal, value);
            }
        }
        [Required]
        public decimal LineTotal
        {
            get
            {
                if (UnitQuantity == null)
                    return (decimal)0;
                return (decimal) (UnitQuantity * EachPrice);
            }
            set { SetValue(() => LineTotal, value); }
        }
        [Required]
        //[Range(0, 10000000)]
        public decimal ItemCurrentQuantity
        {
            get
            {
                return ItemQuantity != null ? ItemQuantity.QuantityOnHand : 0;
            }
            set
            {
                SetValue(() => ItemCurrentQuantity, value);
                SetValue(() => Difference, value);
            }
        }
        [Required]
        //[Range(1, 10000000)]
        public decimal Difference
        {
            get
            {
                if(UnitQuantity!=null)
                return (decimal) (ItemCurrentQuantity - UnitQuantity);
                return ItemCurrentQuantity;
            }
            set { SetValue(() => Difference, value); }
        }

    }
}