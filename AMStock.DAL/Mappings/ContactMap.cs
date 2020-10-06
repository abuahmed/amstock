using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class ContactMap : EntityTypeConfiguration<ContactDTO>
    {
        public ContactMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.FullName)
               .IsRequired();
          
            // Table & Column Mappings
            ToTable("Contacts");
            Property(t => t.Id).HasColumnName("Id");

            //Relationships
            HasOptional(t => t.Address)
              .WithMany()
              .HasForeignKey(t => t.AddressId);
        }
    }
}

