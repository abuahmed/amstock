using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AMStock.Core.Enumerations;

namespace AMStock.Core.Models
{
    public class PhysicalInventoryLineDTO : CommonFieldsA
    {
        [ForeignKey("PhysicalInventory")]
        public int PhysicalInventoryId { get; set; }
        public virtual PhysicalInventoryHeaderDTO PhysicalInventory
        {
            get { return GetValue(() => PhysicalInventory); }
            set { SetValue(() => PhysicalInventory, value); }
        }

        [ForeignKey("Item")]
        public int ItemId { get; set; }
        public ItemDTO Item
        {
            get { return GetValue(() => Item); }
            set { SetValue(() => Item, value); }
        }

        [Required]
        public decimal ExpectedQty
        {
            get { return GetValue(() => ExpectedQty); }
            set { SetValue(() => ExpectedQty, value); }
        }

        [Required]
        public decimal CountedQty
        {
            get { return GetValue(() => CountedQty); }
            set { SetValue(() => CountedQty, value); }
        }

        public PhysicalInventoryLineTypes PhysicalInventoryLineType
        {
            get { return GetValue(() => PhysicalInventoryLineType); }
            set { SetValue(() => PhysicalInventoryLineType, value); }
        }
    }
}
