using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMStock.Core.Models
{
    public class BusinessPartnerContactDTO : UserEntityBase
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
        [ForeignKey("Contact")]
        public int ContactId { get; set; }
        public ContactDTO Contact
        {
            get { return GetValue(() => Contact); }
            set { SetValue(() => Contact, value); }
        }
    }
}