using ActivosFijos.Models;
using Helpers;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace EDUES_ADMIN.Filters
{
    public class AdminFilters
    {

        // Si no estamos logeado, regresamos al login
        public class AutenticadoAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                base.OnActionExecuting(filterContext);

                if (!SessionHelper.ExistUserInSession())
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        controller = "Auth",
                        action = "Index"
                    }));
                    return;
                }

                var controller = (filterContext.RouteData.Values["controller"] ?? string.Empty).ToString();
                var action = (filterContext.RouteData.Values["action"] ?? string.Empty).ToString();

                if (string.Equals(controller, "Usuarios", System.StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(action, "CambiarPassword", System.StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                var userId = SessionHelper.GetUser();
                using (var ctx = new ModelContext())
                {
                    var requiereCambio = ctx.PLU_CONF_Usuario
                        .Where(x => x.IdUsuario == userId)
                        .Select(x => x.ForcePasswordChange)
                        .FirstOrDefault();

                    if (requiereCambio)
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                        {
                            controller = "Usuarios",
                            action = "CambiarPassword"
                        }));
                    }
                }
            }
        }

        // Si estamos logeado ya no podemos acceder a la página de Login
        public class NoLoginAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                base.OnActionExecuting(filterContext);

                if (SessionHelper.ExistUserInSession())
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        controller = "Home",
                        action = "Index"
                    }));
                }
            }
        }

        
    }
}
