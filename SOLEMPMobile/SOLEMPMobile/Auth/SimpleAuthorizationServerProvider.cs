using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Libreria;
namespace SOLEMPMobile
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });


            DBFHelper dbf = new DBFHelper(Properties.Settings.Default.CaminoComun);
            string askUserPass = await dbf.chkUserAndPasswordAsync(context.UserName, context.Password);
            if (askUserPass == null)
            {
                context.SetError("invalid_grant", "El usuario o contraseña son incorrectos.");
                return;
            }
            if ((askUserPass != "Correcto") && (askUserPass != null))
            {
                context.SetError("invalid_grant", "Hubo un problema de comunicación para verificar la Autenticación.");
                return;
            }
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim("sub", context.UserName));
            identity.AddClaim(new Claim("role", "user"));
            context.Validated(identity);

        }
    }
}