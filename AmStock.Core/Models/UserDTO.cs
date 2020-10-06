using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AMStock.Core.Enumerations;
using AMStock.Core.CustomValidationAttributes;
using AMStock.Core.Models.Interfaces;

namespace AMStock.Core.Models
{
    [Table("Users")]
    public class UserDTO : UserEntityBase
    {
        public UserDTO()
        {
            Roles = new HashSet<UsersInRoles>();
            Status = UserTypes.Waiting;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 1)]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        [MinLength(6, ErrorMessage = "User Name Can't be less than Six charactes")]
        [MaxLength(10, ErrorMessage = "User Name Can't be greater than 10 charactes")]
        [ExcludeChar("/.,!@#$%", ErrorMessage = "Contains invalid letters")]
        public string UserName
        {
            get { return GetValue(() => UserName); }
            set { SetValue(() => UserName, value); }
        }

        [DataType(DataType.EmailAddress)]
        [MaxLength(50, ErrorMessage = "Exceeded 50 letters")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is invalid")]
        [StringLength(50)]
        public string Email
        {
            get { return GetValue(() => Email); }
            set { SetValue(() => Email, value); }
        }

        [NotMapped]
        public string Password
        {
            get { return GetValue(() => Password); }
            set { SetValue(() => Password, value); }
        }

        [DataType(DataType.Password)]
        [NotMapped]
        public string ConfirmPassword
        {
            get { return GetValue(() => ConfirmPassword); }
            set { SetValue(() => ConfirmPassword, value); }
        }

        //[Required]
        [StringLength(150)]
        [DisplayName("Full Name")]
        [MaxLength(150, ErrorMessage = "Exceeded 150 letters")]
        public string FullName
        {
            get { return GetValue(() => FullName); }
            set { SetValue(() => FullName, value); }
        }

        [NotMapped]
        public string NewPassword
        {
            get { return GetValue(() => NewPassword); }
            set { SetValue(() => NewPassword, value); }
        }

        //[Required]
        public UserTypes? Status
        {
            get { return GetValue(() => Status); }
            set { SetValue(() => Status, value); }
        }

        [ForeignKey("Organization")]
        public int? OrganizationId { get; set; }//NULL means ALL
        public OrganizationDTO Organization
        {
            get { return GetValue(() => Organization); }
            set { SetValue(() => Organization, value); }
        }

        [ForeignKey("Warehouse")]
        public int? WarehouseId { get; set; }//NULL means ALL
        public WarehouseDTO Warehouse
        {
            get { return GetValue(() => Warehouse); }
            set { SetValue(() => Warehouse, value); }
        }

        public ICollection<UsersInRoles> Roles
        {
            get { return GetValue(() => Roles); }
            set { SetValue(() => Roles, value); }
        }
    }

    [Table("webpages_Membership")]
    public class MembershipDTO : UserEntityBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 0)]
        public int Id { get; set; }

        [Key]
        [Column(Order = 1)]
        public int UserId { get; set; }
        public DateTime? CreateDate { get; set; }
        [MaxLength(255, ErrorMessage = "Exceeded 255 letters")]
        public string ConfirmationToken { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime? LastPasswordFailureDate { get; set; }
        public int PasswordFailuresSinceLastSuccess { get; set; }
        [MaxLength(255, ErrorMessage = "Exceeded 255 letters")]
        public string Password { get; set; }
        public DateTime? PasswordChangedDate { get; set; }
        [MaxLength(255, ErrorMessage = "Exceeded 255 letters")]
        public string PasswordSalt { get; set; }
        [MaxLength(255, ErrorMessage = "Exceeded 255 letters")]
        public string PasswordVerificationToken { get; set; }
        public DateTime? PasswordVerificationTokenExpirationDate { get; set; }
    }
}
