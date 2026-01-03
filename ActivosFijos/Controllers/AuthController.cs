using Helpers;
using System.Web.Mvc;
using static EDUES_ADMIN.Filters.AdminFilters;

namespace ActivosFijos.Controllers
{
    public class AuthController : Controller
    {
        AuthHelper Usuario = new AuthHelper();
        // GET: Auth
        [NoLogin]
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Login(string username, string password_text)
        {
            var rm = Usuario.Auth(username, password_text);

            if (rm.response)
            {
                rm.href = "/Home/Index";
            }

            return Json(rm);
        }

        public ActionResult Logout()
        {
            SessionHelper.DestroyUserSession();
            return Redirect("/Auth/Index");
        }

    }
}