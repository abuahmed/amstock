using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class ItemsMovementLineMap : EntityTypeConfiguration<ItemsMovementLineDTO>
    {
        public ItemsMovementLineMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.MovedQuantity)
                .IsRequired();

            // Table & Column Mappings
            ToTable("ItemsMovementLines");
            Property(t => t.Id).HasColumnName("Id");

            //Relationships
            HasRequired(t => t.ItemsMovementHeader)
                .WithMany(t => t.ItemsMovementLines)
                .HasForeignKey(t => new { t.ItemsMovementHeaderId })
                .WillCascadeOnDelete(false);

            HasRequired(t => t.Item)
                .WithMany(t => t.ItemsMovementLines)
                .HasForeignKey(t => new { t.ItemId })
                .WillCascadeOnDelete(false);

        }
    }
}