using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class PhysicalInventoryHeaderMap : EntityTypeConfiguration<PhysicalInventoryHeaderDTO>
    {
        public PhysicalInventoryHeaderMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.PhysicalInventoryNumber)
               .IsRequired()
               .HasMaxLength(45);

            // Table & Column Mappings
            ToTable("PhysicalInventoryHeaders");
            Property(t => t.Id).HasColumnName("Id");

            //Relationships
            HasRequired(t => t.Warehouse)
              .WithMany(t => t.PhysicalInventories)
              .HasForeignKey(t => new { t.WarehouseId })
              .WillCascadeOnDelete(false);
           
        }
    }
}
