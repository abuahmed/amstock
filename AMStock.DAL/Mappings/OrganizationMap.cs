using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class OrganizationMap : EntityTypeConfiguration<OrganizationDTO>
    {
        public OrganizationMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.DisplayName)
                .IsRequired();

            // Table & Column Mappings
            ToTable("Organizations");
            Property(t => t.Id).HasColumnName("Id");

            //Relationships 
            HasRequired(t => t.Client)
                .WithMany(t => t.Organizations)
                .HasForeignKey(t => new { t.ClientId })
                .WillCascadeOnDelete(false);
        }
    }
}