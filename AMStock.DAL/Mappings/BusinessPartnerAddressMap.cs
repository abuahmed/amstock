using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class BusinessPartnerAddressMap : EntityTypeConfiguration<BusinessPartnerAddressDTO>
    {
        public BusinessPartnerAddressMap()
        {
            // Table & Column Mappings
            ToTable("BusinessPartnerAddress");

            //Relationships
            HasRequired(t => t.BusinessPartner)
                .WithMany(e => e.Addresses)
                .HasForeignKey(t => t.BusinessPartnerId);

            HasRequired(t => t.Address)
                .WithMany(e => e.BusinessPartnerAddresses)
                .HasForeignKey(t => t.AddressId);
            
        }
    }
}