using System.ComponentModel.DataAnnotations;
using AMStock.Core.CustomValidationAttributes;

namespace AMStock.Core.Models
{
    public class SalesPersonDTO : CommonFieldsC
    {
        [MaxLength(50, ErrorMessage = "Sales Person Code exceeded 50 letters")]
        [ExcludeChar("/.,!@#$%", ErrorMessage = "Sales Person Code contains invalid letters")]
        public string SalesPersonCode
        {
            get { return GetValue(() => SalesPersonCode); }
            set { SetValue(() => SalesPersonCode, value); }
        }

        public decimal SalesLimit
        {
            get { return GetValue(() => SalesLimit); }
            set { SetValue(() => SalesLimit, value); }
        }
    }
}
