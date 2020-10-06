using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMStock.Core.Models
{
    public class BusinessPartnerAddressDTO : UserEntityBase
    {
        [Key]
        [Column(Order = 1)]
        [ForeignKey("BusinessPartner")]
        public int BusinessPartnerId { get; set; }
        public BusinessPartnerDTO BusinessPartner
        {
            get { return GetValue(() => BusinessPartner); }
            set { SetValue(() => BusinessPartner, value); }
        }

        [Key]
        [Column(Order = 2)]
        [ForeignKey("Address")]
        public int AddressId { get; set; }
        public AddressDTO Address
        {
            get { return GetValue(() => Address); }
            set { SetValue(() => Address, value); }
        }
    }
}