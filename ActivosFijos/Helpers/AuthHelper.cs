using ActivosFijos.Models;
using System;
using System.Diagnostics;
using System.Linq;


namespace Helpers
{
    public class AuthHelper
    {
        public ResponseModel Auth(string user, string pass)
        {

            var rm = new ResponseModel();

            try
            {
                using (var ctx = new ModelContext())
                {
                    var pass_hash = HashHelper.SHA256(pass);
                    var usuario = ctx.PLU_CONF_Usuario
                                  .Where(x => x.Username == user)
                                  .Where(x => x.Pass == pass_hash)
                                  .Where(x => x.Activo == true).FirstOrDefault();

                    if (usuario != null)
                    {
                        SessionHelper.AddUserToSession(usuario.IdUsuario.ToString());
                        rm.SetResponse(true);
                    }
                    else
                    {
                        rm.SetResponse(false, "Correo o Contraseña Incorrecta");
                    }
                }
            }
            catch (Exception e)
            {
                // TODO: Reemplazar por un proveedor de logging estructurado
                Trace.TraceError($"Error autenticando usuario {user}: {e}");
                rm.SetResponse(false, "No se pudo iniciar sesión en este momento. Inténtalo nuevamente.");
            }

            return rm;
        }
    }
}