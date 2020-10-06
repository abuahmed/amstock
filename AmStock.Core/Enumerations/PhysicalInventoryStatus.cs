using System.ComponentModel;

namespace AMStock.Core.Enumerations
{
    public enum PhysicalInventoryStatus
    {
        [Description("Draft Status")]
        Draft = 0,
        [Description("Posted Status")]
        Posted = 1,
    }
    
    public enum PhysicalInventoryLineTypes
    {
        [Description("After PI")]
        AfterPi = 0,
        [Description("Item Entry")]
        ItemEntry = 1,
        [Description("Unpost Sales")]
        UnpostSales = 2,
        [Description("Unpost Purchase")]
        UnpostPurchase = 3,
    }
}