using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class SalesPersonMap : EntityTypeConfiguration<SalesPersonDTO>
    {
        public SalesPersonMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            //Property(t => t.DisplayName)
            //   .IsRequired()
            //   .HasMaxLength(45);

            Property(t => t.SalesPersonCode)
                .IsRequired();

            // Table & Column Mappings
            ToTable("SalesPersons");
            Property(t => t.Id).HasColumnName("Id");

            //Relationships
            HasOptional(t => t.Address)
              .WithMany()
              .HasForeignKey(t => new { t.AddressId });

        }
    }
}
