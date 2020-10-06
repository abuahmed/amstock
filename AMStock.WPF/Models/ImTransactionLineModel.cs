using System.ComponentModel.DataAnnotations;
using AMStock.Core.Models;

namespace AMStock.WPF.Models
{
    public class ImTransactionLineModel : EntityBase
    {
        public ItemDTO Item
        {
            get { return GetValue(() => Item); }
            set { SetValue(() => Item, value); }
        }

        [Required]
        [Range(0, 10000000)]
        public decimal ItemMovedQuantity
        {
            get { return GetValue(() => ItemMovedQuantity); }
            set
            {
                SetValue(() => ItemMovedQuantity, value);
            }
        }

        [Required]
        [Range(0, 10000000)]
        public decimal ItemOriginQuantity
        {
            get
            {
                return GetValue(() => ItemOriginQuantity);
            }
            set
            {
                SetValue(() => ItemOriginQuantity, value);
            }
        }
        [Required]
        [Range(0, 10000000)]
        public decimal ItemDestinationQuantity
        {
            get
            {
                return GetValue(() => ItemDestinationQuantity);
            }
            set
            {
                SetValue(() => ItemDestinationQuantity, value);
            }
        }
     
    }
}