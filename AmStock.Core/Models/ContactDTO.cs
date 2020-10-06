using System.Collections.Generic;
using AMStock.Core.CustomValidationAttributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMStock.Core.Models
{
    public class ContactDTO : CommonFieldsA
    {
        [Required]
        [MaxLength(250, ErrorMessage = "Full Name exceeded 250 letters")]
        [ExcludeChar("/.,!@#$%", ErrorMessage = "Full Name contains invalid letters")]
        public string FullName
        {
            get { return GetValue(() => FullName); }
            set { SetValue(() => FullName, value); }
        }

        [ForeignKey("Title")]
        public int? TitleId { get; set; }
        public CategoryDTO Title
        {
            get { return GetValue(() => Title); }
            set { SetValue(() => Title, value); }
        }

        [ForeignKey("Position")]
        public int? PositionId { get; set; }
        public CategoryDTO Position
        {
            get { return GetValue(() => Position); }
            set { SetValue(() => Position, value); }
        }

        [MaxLength(50, ErrorMessage = "Suffix exceeded 50 letters")]
        [ExcludeChar("/.,!@#$%", ErrorMessage = "Suffix contains invalid letters")]
        public string Suffix
        {
            get { return GetValue(() => Suffix); }
            set { SetValue(() => Suffix, value); }
        }

        [ForeignKey("Address")]
        public int? AddressId { get; set; }
        public AddressDTO Address
        {
            get { return GetValue(() => Address); }
            set { SetValue(() => Address, value); }
        }

        public ICollection<BusinessPartnerContactDTO> BusinessPartnerContacts
        {
            get { return GetValue(() => BusinessPartnerContacts); }
            set { SetValue(() => BusinessPartnerContacts, value); }
        }
    }
}
