using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class BankGuarnteeMap : EntityTypeConfiguration<BankGuaranteeDTO>
    {
        public BankGuarnteeMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
     
            Property(t => t.BankName)
                .IsRequired();
            
            // Table & Column Mappings
            ToTable("BankGuarntees");
            Property(t => t.Id).HasColumnName("Id");

        }
    }
}