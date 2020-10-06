using System.ComponentModel.DataAnnotations;

namespace AMStock.Core
{
    public class CommonFieldsE : CommonFieldsD
    {
        [MaxLength]
        public byte[] Header
        {
            get { return GetValue(() => Header); }
            set { SetValue(() => Header, value); }
        }
        [MaxLength]
        public byte[] Footer
        {
            get { return GetValue(() => Footer); }
            set { SetValue(() => Footer, value); }
        }
    }
}