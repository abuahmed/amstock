using System.Data.Entity.ModelConfiguration;
using AMStock.Core.Models;

namespace AMStock.DAL.Mappings
{
    public class UserMap : EntityTypeConfiguration<UserDTO>
    {
        public UserMap()
        {
            // Primary Key
            HasKey(t => t.UserId);

            // Properties
            Property(t => t.UserName)
               .IsRequired()
               .HasMaxLength(50);

            // Table & Column Mappings
            ToTable("Users");

            //HasMany(u => u.Roles)
            //    .WithMany()
            //    .Map(a => a.ToTable("UserRoles")
            //                .MapLeftKey("UserId")
            //                .MapRightKey("RoleId"));
        }
    }

    public class MembershipMap : EntityTypeConfiguration<MembershipDTO>
    {
        public MembershipMap()
        {
            // Primary Key
            HasKey(t => t.UserId);

            // Properties
            Property(t => t.Password)
                .IsRequired();

            // Table & Column Mappings
            ToTable("webpages_Membership");

            //Relationships
            //HasMany(u => u.Roles)
            //    .WithMany()
            //    .Map(a => a.ToTable("UserRoles")
            //                .MapLeftKey("UserId")
            //                .MapRightKey("RoleId"));
        }
    }
}
