using System;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using System.Web.Security;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.Service;
using AMStock.Web.Filters;
using AMStock.Web.Models;
using WebMatrix.WebData;

namespace AMStock.Web.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public ActionResult Login(string returnUrl, string recoverystat)
        {
            if (!string.IsNullOrEmpty(recoverystat) && recoverystat == "1")
                ViewData["RecoveryStatHidden"] = "visible";
            else
                ViewData["RecoveryStatHidden"] = "hidden";

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            ViewData["RecoveryStatHidden"] = "hidden";

            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, model.RememberMe))//user != null
            {
                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();
            return Redirect("/Account/Login");

        }

        public ActionResult UserProfile()
        {
            var userProfile = new UserService(true).GetByName(@User.Identity.Name);
            return View(userProfile);
        }

        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UserProfile(UserDTO userProfile)
        {
            if (ModelState.IsValid)
            {

            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterConfimation(string un, string ct)
        {
            return RedirectToAction(WebSecurity.ConfirmAccount(un, ct) ? "Login" : "ConfirmationFailure");
        }

        [AllowAnonymous]
        public ActionResult ConfirmationFailure()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(LoginModel model)
        {
            string userName = model.UserName;
            //check user existance
            //var user = new UserService(false).GetByName(userName);
            var user = Membership.GetUser(userName);
            if (user == null)
            {
                TempData["Message"] = "User Not exist.";
            }
            else
            {
                //generate password token
                var token = WebSecurity.GeneratePasswordResetToken(userName);

                //create url with above token
                var resetLink = "<a href='" + Url.Action("ResetPassword", "Account", new { un = userName, rt = token }, "http") + "'>Reset Password</a>";

                //get user emailid
                var email = new UserService(true).GetByName(userName).Email;// user.Email;

                //send mail
                const string subject = "Password Reset Token";
                var body = "<b>Please find the Password Reset Token</b><br/>" + resetLink; //edit it
                try
                {
                    SendEMail(email, subject, body);
                    TempData["Message"] = "Mail Sent.";
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Error occured while sending email." + ex.Message;
                }
                //only for testing
                TempData["Message"] = resetLink;
            }
            return RedirectToLocal("/Account/Login?recoverystat=1");
            //return View();
        }

        public ActionResult ChangePassword(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed." : "";

            ViewBag.ReturnUrl = Url.Action("ChangePassword");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(LocalPasswordModel model)
        {

            ViewBag.ReturnUrl = Url.Action("ChangePassword");

            if (ModelState.IsValid)
            {
                // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("ChangePassword", new { Message = ManageMessageId.ChangePasswordSuccess });
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult SetPassword(ManageMessageId? message, string token)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.SetPasswordSuccess ? "Your password has been set." : "";

            ViewBag.ReturnUrl = Url.Action("SetPassword");
            var model = new LocalPasswordModel
            {
                ResetToken = token,
                OldPassword = "randomuser"
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult SetPassword(LocalPasswordModel model)
        {
            ViewBag.ReturnUrl = Url.Action("SetPassword");

            if (ModelState.IsValid)
            {
                // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                bool setPasswordSucceeded;
                try
                {
                    setPasswordSucceeded = WebSecurity.ResetPassword(model.ResetToken, model.NewPassword);
                }
                catch (Exception)
                {
                    setPasswordSucceeded = false;
                }

                if (setPasswordSucceeded)
                {
                    return RedirectToAction("SetPassword", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        
        [AllowAnonymous]
        public ActionResult ResetPassword(string un, string rt)
        {
            //get userid of received username
            var userid = new UserService(true).GetByName(un).UserId;

            //check userid and token matches
            bool any;
            using (var dbCon = DbContextUtil.GetDbContextInstance())
            {
                any = dbCon.Set<MembershipDTO>().Any(m => m.UserId == userid
                                                       && (m.PasswordVerificationToken == rt));
                    //&& (m.PasswordVerificationTokenExpirationDate < DateTime.Now)
            }

            return any ? RedirectToAction("SetPassword", new { token = rt }) : RedirectToAction("ForgotPassword");
        }

        private static void SendEMail(string email, string subject, string body)
        {
            //arms@amihanit.com umuahmed11
            const string stringFromPass = "****";
            var smtp = new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true,
                Host = "smtp.gmail.com",
                Port = 465
            };

            var secureString = new System.Security.SecureString();
            stringFromPass.ToCharArray().ToList().ForEach(p => secureString.AppendChar(p));
            var credentials = new System.Net.NetworkCredential("ibrayas@gmail.com", secureString);
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = credentials;

            var msg = new MailMessage
            {
                From = new MailAddress("ibrayas@gmail.com")
            };
            msg.To.Add(new MailAddress(email));

            msg.Subject = subject;
            msg.IsBodyHtml = true;
            msg.Body = body;

            smtp.Send(msg);
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion

    }
}
