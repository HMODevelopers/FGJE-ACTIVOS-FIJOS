using ActivosFijos.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace Helpers
{
    public class SessionHelper
    {
        

        public static bool ExistUserInSession()
        {
            return HttpContext.Current.User.Identity.IsAuthenticated;
        }

        public static void DestroyUserSession()
        {
            FormsAuthentication.SignOut();
        }

        public static int GetUser()
        {
            int user_id = 0;
            if (HttpContext.Current.User != null && HttpContext.Current.User.Identity is FormsIdentity)
            {
                FormsAuthenticationTicket ticket = ((FormsIdentity)HttpContext.Current.User.Identity).Ticket;
                if (ticket != null)
                {
                    user_id = Convert.ToInt32(ticket.UserData);
                }
            }
            return user_id;
        }

        public static string GetUserName(int id)
        {
            using ( ModelContext _db = new ModelContext())
            {
                var usuario = _db.PLU_CONF_Usuario.Where(x => x.IdUsuario == id).Select(x => x.Username).FirstOrDefault();
                return usuario;
            }
          
        }

        public static int GetRol(int id)
        {
            using (ModelContext _db = new ModelContext())
            {
                var usuario = _db.PLU_CONF_Usuario.Where(x => x.IdUsuario == id).Select(x => x.IdRol).FirstOrDefault();
                return usuario;
            }
        }


        public static void AddUserToSession(string id)
        {
            bool persist = true;
            var cookie = FormsAuthentication.GetAuthCookie("Admin", persist);

            cookie.Name = FormsAuthentication.FormsCookieName;
            cookie.Expires = DateTime.Now.AddMonths(3);

            var ticket = FormsAuthentication.Decrypt(cookie.Value);
            var newTicket = new FormsAuthenticationTicket(ticket.Version, ticket.Name, ticket.IssueDate, ticket.Expiration, ticket.IsPersistent, id);

            cookie.Value = FormsAuthentication.Encrypt(newTicket);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

       
    }
}