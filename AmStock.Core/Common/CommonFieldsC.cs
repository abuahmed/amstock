using System.ComponentModel.DataAnnotations.Schema;
using AMStock.Core.Models;

namespace AMStock.Core
{
    public class CommonFieldsC : CommonFieldsB
    {
        [ForeignKey("Address")]
        public int? AddressId { get; set; }
        public AddressDTO Address
        {
            get { return GetValue(() => Address); }
            set { SetValue(() => Address, value); }
        }
    }
}
