using System.Web.Mvc;
using AMStock.Core.Models;
using AMStock.Service;

namespace AMStock.Web.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        [Authorize(Roles = "UsersMgmt")]
        public ActionResult Index()
        {
            var users = new UserService(true).GetAll();
            #region For Serial Number
            var sNo = 1;//(criteria.Page - 1) * criteria.PageSize + 1;
            foreach (var user in users)
            {
                user.SerialNumber = sNo;
                sNo++;
            }
            #endregion
            return View(users);
        }

        [Authorize(Roles = "UsersMgmt")]
        public ActionResult Details(int id)
        {
            var model = new UserDTO();
            if (id != 0)
                model = new UserService(true).GetUser(id);

            return View(model);
        }
    }
}
