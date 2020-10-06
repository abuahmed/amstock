using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMStock.Core.Models
{
    [Table("webpages_Roles")]
    public class RoleDTO : UserEntityBase
    {
        public RoleDTO()
        {
            Users = new List<UsersInRoles>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 1)]
        public int RoleId { get; set; }
        
        [Required]
        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        public string RoleName { get; set; }
        
        [StringLength(255)]
        [MaxLength(255, ErrorMessage = "Exceeded 255 letters")]
        public string RoleDescription
        {
            get { return GetValue(() => RoleDescription); }
            set { SetValue(() => RoleDescription, value); }
        }

        [MaxLength(255, ErrorMessage = "Exceeded 255 letters")]
        public string RoleDescriptionShort
        {
            get { return GetValue(() => RoleDescriptionShort); }
            set { SetValue(() => RoleDescriptionShort, value); }
        }
        
        public ICollection<UsersInRoles> Users
        {
            get { return GetValue(() => Users); }
            set { SetValue(() => Users, value); }
        }
    }
    [Table("webpages_UsersInRoles")]
    public class UsersInRoles : UserEntityBase
    {
        [Key]
        [Column(Order = 1)]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public UserDTO User
        {
            get { return GetValue(() => User); }
            set { SetValue(() => User, value); }
        }

        [Key]
        [Column(Order = 2)]
        [ForeignKey("Role")]
        public int RoleId { get; set; }
        public RoleDTO Role
        {
            get { return GetValue(() => Role); }
            set { SetValue(() => Role, value); }
        }
    }
}
