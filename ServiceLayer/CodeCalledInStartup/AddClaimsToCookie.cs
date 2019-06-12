// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataLayer.EfCode;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceLayer.CodeCalledInStartup
{
    public static class AddClaimsToCookie
    {
        /// <summary>
        /// This configures Cookies for authentication and adds the feature and data claims to the user
        /// </summary>
        /// <param name="services"></param>
        /// <param name="updateCookieOnChange">if false then uses simple method to set up the claims,
        /// otherwise uses OnValidatePrincipal to allow the claims to be changed.</param>
        public static void ConfigureCookiesForExtraAuth(this IServiceCollection services, bool updateCookieOnChange)
        {

            if (updateCookieOnChange)
            {
                var sp = services.BuildServiceProvider();
                var extraAuthContextOptions = sp.GetRequiredService<DbContextOptions<ExtraAuthorizeDbContext>>();

                //TODO add update on feature change to AuthCookieValidate
                var authCookieValidate = new AuthCookieValidate(
                    new CalcAllowedPermissions(extraAuthContextOptions), new CalcDataKey(extraAuthContextOptions));

                //see https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-2.1#cookie-settings
                services.ConfigureApplicationCookie(options =>
                {
                    options.Events.OnValidatePrincipal = authCookieValidate.ValidateAsync;
                });
            }
            else
            {
                //Simple version - see https://korzh.com/blogs/net-tricks/aspnet-identity-store-user-data-in-claims
                services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>, AddPermissionsToUserClaims>();
            }
        }
    }
}