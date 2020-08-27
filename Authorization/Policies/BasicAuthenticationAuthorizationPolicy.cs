namespace Nop.Plugin.Api.Authorization.Policies
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Nop.Plugin.Api.Authorization.Requirements;

    public class BasicAuthenticationAuthorizationPolicy : AuthorizationHandler<BasicAuthenticationRequirement>
    {
        IHttpContextAccessor _httpContextAccessor = null;

        public BasicAuthenticationAuthorizationPolicy(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, BasicAuthenticationRequirement requirement)
        {
            HttpContext httpContext = _httpContextAccessor.HttpContext;

            if (requirement.HasAccess(httpContext))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}