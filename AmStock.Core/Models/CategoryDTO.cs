using AMStock.Core.Enumerations;

namespace AMStock.Core.Models
{
    public class CategoryDTO : CommonFieldsB
    {
        public NameTypes NameType
        {
            get { return GetValue(() => NameType); }
            set { SetValue(() => NameType, value); }
        }
    }
}
