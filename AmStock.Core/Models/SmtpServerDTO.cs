using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AMStock.Core.Models
{
    public class SmtpServerDTO : CommonFieldsA
    {
        [DisplayName("Smtp Server")]
        [Required]
        [MaxLength(255, ErrorMessage = "Exceeded 255 letters")]
        public string SmtpServer
        {
            get { return GetValue(() => SmtpServer); }
            set { SetValue(() => SmtpServer, value); }
        }

        [DisplayName("Server Account")]
        [Required]
        [MaxLength(255, ErrorMessage = "Exceeded 255 letters")]
        public string Account
        {
            get { return GetValue(() => Account); }
            set { SetValue(() => Account, value); }
        }

        [DisplayName("Server Password")]
        [MaxLength(255, ErrorMessage = "Exceeded 255 letters")]
        public string Password
        {
            get { return GetValue(() => Password); }
            set { SetValue(() => Password, value); }
        }

        [DisplayName("Server Sender Address")]
        [MaxLength(255, ErrorMessage = "Exceeded 255 letters")]
        public string SenderAddress
        {
            get { return GetValue(() => SenderAddress); }
            set { SetValue(() => SenderAddress, value); }
        }

        [MaxLength(255, ErrorMessage = "Exceeded 255 letters")]
        public string ConnectionSecurity
        {
            get { return GetValue(() => ConnectionSecurity); }
            set { SetValue(() => ConnectionSecurity, value); }
        }

        [DisplayName("SMTP Port")]
        [MaxLength(255, ErrorMessage = "Exceeded 255 letters")]
        public string Port
        {
            get { return GetValue(() => Port); }
            set { SetValue(() => Port, value); }
        }
    }
}
