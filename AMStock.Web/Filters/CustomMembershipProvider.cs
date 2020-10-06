using System.Web.Security;
using AMStock.Service;
using WebMatrix.WebData;

namespace AMStock.Web.Filters
{
    public class CustomMembershipProvider : SimpleMembershipProvider
    {
        public override bool ValidateUser(string username, string password)
        {
            //var userService = new UserService(true);
            //var user = userService.Login(username, password);
            //return user != null;
            return base.ValidateUser(username, password);
        }

        //public override MembershipUser GetUser(string username, bool userIsOnline)
        //{
        //    var userService = new UserService(true);
        //    var user = userService.GetByName(username);
        //    return user;
        //    //return base.GetUser(username, userIsOnline);
        //}
        public override string GeneratePasswordResetToken(string userName)
        {
            return base.GeneratePasswordResetToken(userName);
        }
    }

    public class CustomRoleProvider : SimpleRoleProvider
    {
        public override void CreateRole(string roleName)
        {
            base.CreateRole(roleName);
        }
    }
}