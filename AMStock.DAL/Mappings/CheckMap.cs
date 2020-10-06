using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class CheckMap : EntityTypeConfiguration<CheckDTO>
    {
        public CheckMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.CheckDate)
               .IsRequired();

            Property(t => t.CheckNumber)
                .IsRequired();
            
            // Table & Column Mappings
            ToTable("Checks");
            Property(t => t.Id).HasColumnName("Id");

            //Relationships 

        }
    }
}
