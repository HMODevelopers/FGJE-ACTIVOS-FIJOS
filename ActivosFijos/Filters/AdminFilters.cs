using Helpers;
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