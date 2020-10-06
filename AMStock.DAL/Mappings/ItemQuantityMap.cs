using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class ItemQuantityMap : EntityTypeConfiguration<ItemQuantityDTO>
    {
        public ItemQuantityMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.QuantityOnHand)
               .IsRequired();

            // Table & Column Mappings
            ToTable("ItemQuantities");
            Property(t => t.Id).HasColumnName("Id");

            //Relationships
            HasRequired(t => t.Warehouse)
              .WithMany()
              .HasForeignKey(t => new { t.WarehouseId })
              .WillCascadeOnDelete(false);

            HasRequired(t => t.Item)
              .WithMany(t=>t.ItemQuantities)
              .HasForeignKey(t => new { t.ItemId })
              .WillCascadeOnDelete(false);
        }
    }
}
