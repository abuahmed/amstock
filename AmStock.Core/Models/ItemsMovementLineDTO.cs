using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMStock.Core.Models
{
    public class ItemsMovementLineDTO : CommonFieldsA
    {
        public ItemsMovementLineDTO()
        {
            MovedQuantity = 0;
        }

        [ForeignKey("ItemsMovementHeader")]
        public int ItemsMovementHeaderId { get; set; }
        public ItemsMovementHeaderDTO ItemsMovementHeader
        {
            get { return GetValue(() => ItemsMovementHeader); }
            set { SetValue(() => ItemsMovementHeader, value); }
        }

        [ForeignKey("Item")]
        public int ItemId { get; set; }
        public ItemDTO Item
        {
            get { return GetValue(() => Item); }
            set { SetValue(() => Item, value); }
        }

        public decimal OriginPreviousQuantity
        {
            get { return GetValue(() => OriginPreviousQuantity); }
            set { SetValue(() => OriginPreviousQuantity, value); }
        }

        public decimal DestinationPreviousQuantity
        {
            get { return GetValue(() => DestinationPreviousQuantity); }
            set { SetValue(() => DestinationPreviousQuantity, value); }
        }

        [Range(1, 1000000)]
        public decimal MovedQuantity
        {
            get { return GetValue(() => MovedQuantity); }
            set { SetValue(() => MovedQuantity, value); }
        }
    }
}