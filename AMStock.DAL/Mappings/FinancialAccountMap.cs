using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class FinancialAccountMap : EntityTypeConfiguration<FinancialAccountDTO>
    {
        public FinancialAccountMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.AccountNumber)
               .IsRequired();

            Property(t => t.BankName)
                .IsRequired();

            Property(t => t.BankBranch)
                .IsRequired();

            // Table & Column Mappings
            ToTable("FinancialAccounts");
            Property(t => t.Id).HasColumnName("Id");

            //Relationships
            //HasOptional(t => t.Warehouse)
            //                 .WithMany(t => t.FinancialAccounts)
            //                 .HasForeignKey(t => new { t.WarehouseId })
            //                 .WillCascadeOnDelete(false);

            //HasOptional(t => t.BusinessPartner)
            //                  .WithMany(t => t.FinancialAccounts)
            //                  .HasForeignKey(t => new { t.BusinessPartnerId })
            //                  .WillCascadeOnDelete(false);

        }
    }
}
