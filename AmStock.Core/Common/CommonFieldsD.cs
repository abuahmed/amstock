using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AMStock.Core
{
    public class CommonFieldsD : CommonFieldsC
    {
        [MaxLength(255, ErrorMessage = "Contact Title Exceeded 255 letters")]
        [DisplayName("Contact Title")]
        public string ContactTitle
        {
            get { return GetValue(() => ContactTitle); }
            set { SetValue(() => ContactTitle, value); }
        }

        [MaxLength(255, ErrorMessage = "Contact Name Exceeded 255 letters")]
        [DisplayName("Contact Name")]
        public string ContactName
        {
            get { return GetValue(() => ContactName); }
            set { SetValue(() => ContactName, value); }
        }
    }
}