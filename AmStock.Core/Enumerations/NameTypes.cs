using System.ComponentModel;

namespace AMStock.Core.Enumerations
{
    public enum NameTypes
    {
        [Description("Item Category")]
        Category = 0,
        [Description("Unit Of Measure")]
        UnitMeasure = 1,
        [Description("Customer Category")]
        CustomerCategory = 2,
        [Description("List of Countries")]
        Country = 3,
        [Description("List Of Banks")]
        Bank = 4,
        [Description("Address Types")]
        AddressType = 5,
        [Description("Contact Types")]
        ContactType = 6,
        [Description("City")]
        City = 7,
        [Description("SubCity")]
        SubCity = 8,
        [Description("Titles")]
        Title = 9,
        [Description("Positions")]
        Position = 10
    }
}
