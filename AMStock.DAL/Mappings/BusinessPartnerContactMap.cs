using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class BusinessPartnerContactMap : EntityTypeConfiguration<BusinessPartnerContactDTO>
    {
        public BusinessPartnerContactMap()
        {
            // Table & Column Mappings
            ToTable("BusinessPartnerContacts");

            //Relationships
            HasRequired(t => t.BusinessPartner)
                .WithMany(e => e.Contacts)
                .HasForeignKey(t => t.BusinessPartnerId);

            HasRequired(t => t.Contact)
                .WithMany(e => e.BusinessPartnerContacts)
                .HasForeignKey(t => t.ContactId);
        }
    }
}