using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class ErrorLogMap : EntityTypeConfiguration<ErrorLogDTO>
    {
        public ErrorLogMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties

            // Table & Column Mappings
            ToTable("ErrorLog");
            Property(t => t.Id).HasColumnName("Id");

            //Relationships 
        }
    }
}