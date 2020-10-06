using System.ComponentModel;

namespace AMStock.Core.Enumerations
{
    public enum ClientTypes
    {
        [Description("Single Org. Single Store")]
        SingleOrgSingleStore = 0,
        [Description("Single Org. Multi Stores")]
        SingleOrgMultiStore = 1,
        [Description("Multi Org. Single Store")]
        MultiOrgSingleStore = 2,
        [Description("Multi Org. Multi Stores")]
        MultiOrgMultiStore = 3
    }

    //public enum AMStockFeatures//No need can be handled by setting DTO
    //{
    //    [Description("Single Store With Full Feature")]
    //    FullAccessSingleStore = 0,
    //    [Description("All Store With Full Feature")]
    //    FullAccessAllStore = 1
    //}

    public enum ItemTypes
    {
        Purchased = 0,
        Manufactured = 1,
        Service = 2
    }
}