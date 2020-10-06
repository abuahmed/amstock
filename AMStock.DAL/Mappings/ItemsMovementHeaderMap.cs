using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class ItemsMovementHeaderMap : EntityTypeConfiguration<ItemsMovementHeaderDTO>
    {
        public ItemsMovementHeaderMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.MovementNumber)
                .IsRequired()
                .HasMaxLength(45);

            // Table & Column Mappings
            ToTable("ItemsMovementHeaders");
            Property(t => t.Id).HasColumnName("Id");

            //Relationships
            HasRequired(t => t.FromWarehouse)
                .WithMany()
                .HasForeignKey(t => new { t.FromWarehouseId })
                .WillCascadeOnDelete(false);

            HasRequired(t => t.ToWarehouse)
                .WithMany()
                .HasForeignKey(t => new { t.ToWarehouseId })
                .WillCascadeOnDelete(false);
        }
    }
}