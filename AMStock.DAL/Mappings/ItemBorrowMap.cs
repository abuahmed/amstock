using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class ItemBorrowMap : EntityTypeConfiguration<ItemBorrowDTO>
    {
        public ItemBorrowMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.Quantity)
                .IsRequired();

            // Table & Column Mappings
            ToTable("ItemBorrows");
            Property(t => t.Id).HasColumnName("Id");

            //Relationships
            HasRequired(t => t.Item)
                .WithMany(t => t.ItemBorrows)
                .HasForeignKey(t => new { t.ItemId })
                .WillCascadeOnDelete(false);
        }
    }
}