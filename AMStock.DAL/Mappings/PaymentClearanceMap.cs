using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class PaymentClearanceMap : EntityTypeConfiguration<PaymentClearanceDTO>
    {
        public PaymentClearanceMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.Id)
               .IsRequired();

            Property(t => t.ClientAccountId)
               .IsRequired();

            // Table & Column Mappings
            ToTable("PaymentClearances");
            Property(t => t.Id).HasColumnName("Id");

            HasRequired(t => t.ClientAccount)
                .WithMany()
                .HasForeignKey(t => new { t.ClientAccountId })
                .WillCascadeOnDelete(false);
        }
    }
}
