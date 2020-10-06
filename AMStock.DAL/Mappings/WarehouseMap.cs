using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class WarehouseMap : EntityTypeConfiguration<WarehouseDTO>
    {
        public WarehouseMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            //Property(t => t.DisplayName)
            //   .IsRequired()
            //   .HasMaxLength(45);

            // Table & Column Mappings
            ToTable("Warehouses");
            Property(t => t.Id).HasColumnName("Id");

            //Relationships
            HasRequired(t => t.Organization)
              .WithMany(t => t.Warehouses)
              .HasForeignKey(t => new { t.OrganizationId });

            HasOptional(t => t.Address)
              .WithMany()
              .HasForeignKey(t => new { t.AddressId });


        }
    }
}
