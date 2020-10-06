using System.ComponentModel.DataAnnotations;
//using System.Web.Mvc;
using AMStock.Core.Models;

namespace AMStock.WPF.Models
{
    public class ChangePasswordModel : EntityBase
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string OldPassword
        {
            get { return GetValue(() => OldPassword); }
            set { SetValue(() => OldPassword, value); }
        } 

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]        
        public string Password
        {
            get { return GetValue(() => Password); }
            set { SetValue(() => Password, value); }
        } 

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]        
        public string ConfirmPassword
        {
            get { return GetValue(() => ConfirmPassword); }
            set { SetValue(() => ConfirmPassword, value); }
        } 
    }
}
