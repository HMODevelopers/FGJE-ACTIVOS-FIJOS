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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Login(string username, string password_text)
        {
            var rm = Usuario.Auth(username, password_text);

            if (rm.response)
            {
                var requiereCambio = rm.result is bool && (bool)rm.result;
                rm.href = requiereCambio ? Url.Action("CambiarPassword", "Usuarios", new { returnUrl = "/Home/Index" }) : "/Home/Index";
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
