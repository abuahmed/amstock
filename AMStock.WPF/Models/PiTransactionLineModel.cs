using System.ComponentModel.DataAnnotations;
using AMStock.Core.Models;

namespace AMStock.WPF.Models
{
    public class PiTransactionLineModel : EntityBase
    {
        public ItemDTO Item
        {
            get { return GetValue(() => Item); }
            set { SetValue(() => Item, value); }
        }

        [Required]
        [Range(0, 10000000)]
        public decimal ItemCountedQuantity
        {
            get { return GetValue(() => ItemCountedQuantity); }
            set
            {
                SetValue(() => ItemCountedQuantity, value);
                SetValue(() => Difference, value);
            }
        }
   
        [Required]
        [Range(0, 10000000)]
        public decimal ItemCurrentQuantity
        {
            get
            {
                return GetValue(() => ItemCurrentQuantity);
            }
            set
            {
                SetValue(() => ItemCurrentQuantity, value);
                SetValue(() => Difference, value);
            }
        }

        [Required]
        public decimal Difference
        {
            get
            {
                return ItemCountedQuantity - ItemCurrentQuantity;
            }
            set { SetValue(() => Difference, value); }
        }
    }
}