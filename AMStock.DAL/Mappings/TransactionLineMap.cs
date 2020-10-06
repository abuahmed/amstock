using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class TransactionLineMap : EntityTypeConfiguration<TransactionLineDTO>
    {
        public TransactionLineMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.LineNumber)
                .IsRequired();

            // Table & Column Mappings
            ToTable("TransactionLines");
            Property(t => t.Id).HasColumnName("Id");

            //Relationships
            HasRequired(t => t.Transaction)
                .WithMany(t => t.TransactionLines)
                .HasForeignKey(t => new { t.TransactionId })
                .WillCascadeOnDelete(false);

            HasRequired(t => t.Item)
                .WithMany(t => t.TransactionLines)
                .HasForeignKey(t => new { t.ItemId })
                .WillCascadeOnDelete(false);

        }
    }
}