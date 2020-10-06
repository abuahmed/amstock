using System.ComponentModel.DataAnnotations;

namespace AMStock.Web.Models
{
    public class LoginModel
    {
        [Required]
        [Display(Name = "User name")]
        [MinLength(6)]
        public string UserName { get; set; }

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string ResetToken { get; set; }
    }
    
    //public class RegisterModel
    //{
    //    [ScaffoldColumn(false)]
    //    public int UserId { get; set; }

    //    [Required]
    //    [Display(Name = "User name")]
    //    public string UserName { get; set; }

    //    [Required]
    //    [Display(Name = "Email")]
    //    public string Email { get; set; }

    //    [Required]
    //    [Display(Name = "Full Name")]
    //    public string FullName { get; set; }
        
    //    [Required]
    //    [Display(Name = "Is Active?")]
    //    public bool Active { get; set; }

    //    [Required]
    //    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    //    [DataType(DataType.Password)]
    //    [Display(Name = "Password")]
    //    public string Password { get; set; }

    //    [DataType(DataType.Password)]
    //    [Display(Name = "Confirm password")]
    //    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    //    public string ConfirmPassword { get; set; }
    //}

    //public class UserModel
    //{
    //    [ScaffoldColumn(false)]
    //    public int UserId { get; set; }

    //    [Required]
    //    [Display(Name = "User name")]
    //    public string UserName { get; set; }

    //    [Required]
    //    [Display(Name = "Email")]
    //    public string Email { get; set; }

    //    [Required]
    //    [Display(Name = "Full Name")]
    //    public string FullName { get; set; }
        
    //}

    //public class RolesViewModel
    //{
    //    [ScaffoldColumn(false)]
    //    public int RoleId { get; set; }
    //    public string RoleName { get; set; }
    //    public string RoleDescription { get; set; }
    //    public int RoleCategory { get; set; }
    //}

    //public class UsersViewModel
    //{
    //    [ScaffoldColumn(false)]
    //    public int UserId { get; set; }
    //    public string UserName { get; set; }
    //}

    //public class RegisterExternalLoginModel
    //{
    //    [Required]
    //    [Display(Name = "User name")]
    //    public string UserName { get; set; }

    //    public string ExternalLoginData { get; set; }
    //}

    //public class ExternalLogin
    //{
    //    public string Provider { get; set; }
    //    public string ProviderDisplayName { get; set; }
    //    public string ProviderUserId { get; set; }
    //}

}

