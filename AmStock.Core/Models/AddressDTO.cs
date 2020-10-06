using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMStock.Core.Models
{
    public class AddressDTO : CommonFieldsA
    {
        [ForeignKey("AddressType")]
        public int? AddressTypeId { get; set; }
        public CategoryDTO AddressType
        {
            get { return GetValue(() => AddressType); }
            set { SetValue(() => AddressType, value); }
        }

        [MaxLength(255, ErrorMessage = "Exceeded 255 letters")]
        [Display(Name = "Detail Address")]
        public string AddressDetail
        {
            get { return GetValue(() => AddressDetail); }
            set { SetValue(() => AddressDetail, value); }
        }

        [MaxLength(250, ErrorMessage = "Exceeded 250 letters")]
        [Display(Name = "Street Address")]
        public string StreetAddress
        {
            get { return GetValue(() => StreetAddress); }
            set { SetValue(() => StreetAddress, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression("^[0-9]{10,14}$", ErrorMessage = "Mobile Number is invalid")]
        public string Mobile
        {
            get { return GetValue(() => Mobile); }
            set { SetValue(() => Mobile, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression("^[0-9]{10,14}$", ErrorMessage = "Alternate Mobile Number is invalid")]
        public string AlternateMobile
        {
            get { return GetValue(() => AlternateMobile); }
            set { SetValue(() => AlternateMobile, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Fixed Line Telephone")]
        [RegularExpression("^[0-9]{10,14}$", ErrorMessage = "Telephone is invalid")]
        public string Telephone
        {
            get { return GetValue(() => Telephone); }
            set { SetValue(() => Telephone, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Alternate Fixed Line Telephone")]
        [RegularExpression("^[0-9]{10,14}$", ErrorMessage = "Alternate Telephone is invalid")]
        public string AlternateTelephone
        {
            get { return GetValue(() => AlternateTelephone); }
            set { SetValue(() => AlternateTelephone, value); }
        }
        
        [DataType(DataType.EmailAddress)]
        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is invalid")]
        [Display(Name = "Primary Email")]
        public string PrimaryEmail
        {
            get { return GetValue(() => PrimaryEmail); }
            set { SetValue(() => PrimaryEmail, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is invalid")]
        [Display(Name = "Alternate Email")]
        public string AlternateEmail
        {
            get { return GetValue(() => AlternateEmail); }
            set { SetValue(() => AlternateEmail, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        [DataType(DataType.Url)]
        [RegularExpression("^([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Web Address is invalid")]
        [Display(Name = "Web Address")]
        public string WebAddress
        {
            get { return GetValue(() => WebAddress); }
            set { SetValue(() => WebAddress, value); }
        }
        
        [Required]
        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        public string Country
        {
            get { return GetValue(() => Country); }
            set { SetValue(() => Country, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        public string Region
        {
            get { return GetValue(() => Region); }
            set { SetValue(() => Region, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        public string City
        {
            get { return GetValue(() => City); }
            set { SetValue(() => City, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        [Display(Name = "Sub City")]
        public string SubCity
        {
            get { return GetValue(() => SubCity); }
            set { SetValue(() => SubCity, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        public string Woreda
        {
            get { return GetValue(() => Woreda); }
            set { SetValue(() => Woreda, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        public string Kebele
        {
            get { return GetValue(() => Kebele); }
            set { SetValue(() => Kebele, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        [Display(Name = "House Number")]
        public string HouseNumber
        {
            get { return GetValue(() => HouseNumber); }
            set { SetValue(() => HouseNumber, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        [Display(Name = "P.O.Box")]
        public string PoBox
        {
            get { return GetValue(() => PoBox); }
            set { SetValue(() => PoBox, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        [RegularExpression("^[0-9]{8,14}$", ErrorMessage = "Fax is invalid")]
        public string Fax
        {
            get { return GetValue(() => Fax); }
            set { SetValue(() => Fax, value); }
        }

        [DataType(DataType.MultilineText)]
        public string MoreNote
        {
            get { return GetValue(() => MoreNote); }
            set { SetValue(() => MoreNote, value); }
        }
        
        [NotMapped]
        public string AddressDetailShort
        {
            get
            {
                if (AddressDetail == null)
                    return "";
                if (AddressDetail.Length > 20)
                    return AddressDetail.Substring(0, 20) + "..";
                return AddressDetail;
            }
            set { SetValue(() => AddressDetailShort, value); }
        }

        public ICollection<BusinessPartnerAddressDTO> BusinessPartnerAddresses
        {
            get { return GetValue(() => BusinessPartnerAddresses); }
            set { SetValue(() => BusinessPartnerAddresses, value); }
        }
    }
}
