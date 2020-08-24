using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core.Infrastructure;
using Nop.Plugin.Api.Services;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Primitives;
using Org.BouncyCastle.Asn1.Ocsp;
using Nop.Services.Configuration;

namespace Nop.Plugin.Api.APIAuth
{
    public class BasicAuthenticationAttribute : ActionFilterAttribute
    {       
        public bool AllowMultiple
        {
            get
            {
                return true;
            }
        }
                 
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (actionContext == null)
                throw new ArgumentNullException("httpContext");

            var req = actionContext.HttpContext.Request;

            if (req.Headers.TryGetValue("Authorization", out StringValues auth))
            {
                var authHeader = AuthenticationHeaderValue.Parse(auth);

                var cred = System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(':');
                var user = new { ClientId = cred[0], ClientSecret = cred[1] };

                var settingService = EngineContext.Current.Resolve<ISettingService>();
                var clientsSetting = settingService.GetSettingByKey<string>("nop.plugin.api.clients");
                if(clientsSetting != null)
                {
                    var clients = clientsSetting.Split(';');
                    foreach (var client in clients)
                    {
                        var credentials = client.Split(',');
                        if(credentials.Length == 2 && credentials[0] == user.ClientId && credentials[1] == user.ClientSecret)
                        {
                            return;
                        }
                    }
                }          
            }

            actionContext.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
    }
}
