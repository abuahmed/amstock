using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class CpoMap : EntityTypeConfiguration<CpoDTO>
    {
        public CpoMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.Number)
               .IsRequired();
       
            // Table & Column Mappings
            ToTable("Cpos");
            Property(t => t.Id).HasColumnName("Id");

            //Relationships 
        }
    }
}
