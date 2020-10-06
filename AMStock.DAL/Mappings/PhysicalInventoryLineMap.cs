using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class PhysicalInventoryLineMap : EntityTypeConfiguration<PhysicalInventoryLineDTO>
    {
        public PhysicalInventoryLineMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.CountedQty)
               .IsRequired();           

            // Table & Column Mappings
            ToTable("PhysicalInventoryLines");
            Property(t => t.Id).HasColumnName("Id");

            //Relationships
            HasRequired(t => t.PhysicalInventory)
              .WithMany(t=>t.PhysicalInventoryLines)
              .HasForeignKey(t => new { t.PhysicalInventoryId })
              .WillCascadeOnDelete(false);

            HasRequired(t => t.Item)
              .WithMany(t=>t.PhysicalInventoryLines)
              .HasForeignKey(t => new { t.ItemId })
              .WillCascadeOnDelete(false);

        }
    }
}
