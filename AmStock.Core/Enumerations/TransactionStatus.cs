using System.ComponentModel;

namespace AMStock.Core.Enumerations
{
    public enum TransactionStatus
    {
        [Description("New")]
        New,
        [Description("Draft")]
        Draft,
        [Description("Order Status")]
        Order,
        [Description("Posted Status")]
        Posted,
        [Description("Posted With Less Stock")]
        PostedWithLessStock,
        [Description("Completed")]
        Completed,
        [Description("Closed")]
        Closed,
        [Description("Approved")]
        Approved,
        [Description("Archived")]
        Archived,
        [Description("Cancel")]
        Canceled,
        [Description("On Process")]
        OnProcess,
        [Description("Shipped")]
        Shipped,
        [Description("Delivery Confirmed")]
        DeliveryConfirmed,
        [Description("Received")]
        Received,
        [Description("Refunded Status")]
        Refunded

    }
}