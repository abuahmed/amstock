using System.Web;
using System.Web.Mvc;

namespace AMStock.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            //filters.Add(new InitializeSimpleMembershipAttribute());//we don't need we have our own membership controlling features
        }
    }
}