using System.ComponentModel;

namespace AMStock.Core.Enumerations
{
    public enum ItemBorrowTypes
    {
        [Description("Item Given To")]
        BorrowedTo = 0,
        [Description("Item Accepted From")]
        BorrowedFrom = 1,
    }
}