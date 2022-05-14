using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using ServerPublisher.Server.Managers;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ServerPublisher.Server.Network.WebService.Utils
{
    public class CookieSignFilter : IMiddleware
    {
        public const string IdentityCookie = "Identity";
        public const string IdCookie = "Identity.Id";

        private readonly UserManager userManager;

        public CookieSignFilter(UserManager userManager)
        {
            this.userManager = userManager;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (
                !context.Request.Cookies.TryGetValue(IdentityCookie, out var identityValue) ||
                !context.Request.Cookies.TryGetValue(IdCookie, out var idValue) ||
                !userManager.ValidateUser(idValue, Convert.FromBase64String(identityValue), out var user)
                )
            {
                await next(context);
                return;
            }

            context.User.AddIdentity(new ClaimsIdentity(new List<Claim>()
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType,user.Name)
            }, CookieAuthenticationDefaults.AuthenticationScheme));
        }
    }
}
