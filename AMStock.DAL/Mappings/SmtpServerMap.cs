using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class SmtpServerMap : EntityTypeConfiguration<SmtpServerDTO>
    {
        public SmtpServerMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.Account)
               .IsRequired();

            Property(t => t.SmtpServer)
                .IsRequired();
            
            // Table & Column Mappings
            ToTable("SmtpServers");
            Property(t => t.Id).HasColumnName("Id");

            //Relationships 
           
        }
    }
}
