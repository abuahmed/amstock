using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class BuildVersionMap : EntityTypeConfiguration<BuildVersionDTO>
    {
        public BuildVersionMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties

            // Table & Column Mappings
            ToTable("BuildVersion");
            Property(t => t.Id).HasColumnName("Id");

            //Relationships 
        }
    }
}