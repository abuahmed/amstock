using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMStock.Core.Models
{
    public class TransactionLine : CommonFieldsA
    {
        [ForeignKey("Item")]
        public int ItemId { get; set; }
        public ItemDTO Item
        {
            get { return GetValue(() => Item); }
            set { SetValue(() => Item, value); }
        }

        public int LineNumber
        {
            get { return GetValue(() => LineNumber); }
            set { SetValue(() => LineNumber, value); }
        }

        [Required]
        public decimal Unit
        {
            get { return GetValue(() => Unit); }
            set
            {
                SetValue(() => Unit, value);
                SetValue(() => LinePrice, value);
            }
        }

        [Required]
        [DisplayName("Each Price")]
        public decimal EachPrice
        {
            get { return GetValue(() => EachPrice); }
            set
            {
                SetValue(() => EachPrice, value);
                SetValue(() => LinePrice, value);
            }
        }

        [NotMapped]
        [DisplayName("Line Price")]
        public decimal LinePrice
        {
            get { return Unit * EachPrice; }
            set { SetValue(() => LinePrice, value); }
        }

        [NotMapped]
        [DisplayName("Line Purchase Price")]
        public decimal LinePurchasePrice
        {
            get
            {
                if (Item != null) return Unit * Item.PurchasePrice;
                return 0;
            }
            set { SetValue(() => LinePurchasePrice, value); }
        }
    }
}
