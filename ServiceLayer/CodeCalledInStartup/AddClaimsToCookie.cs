// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using CommonCache;
using DataLayer.EfCode;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.UserImpersonation.Concrete.Internal;

namespace ServiceLayer.CodeCalledInStartup
{
    public static class AddClaimsToCookie
    {
        /// <summary>
        /// This configures Cookies for authentication and adds the feature and data claims to the user.
        /// There are two approaches:
        /// 1. One that allows logged in user's permissions to updated when the Roles/Permissions are changed.
        /// 2. A simpler/better performance way to set up permissions, but doesn't support dynamic updates of logged in user's permissions
        /// </summary>
        /// <param name="services"></param>
        /// <param name="updateCookieOnChange">if false then uses simple method to set up the claims,
        /// otherwise uses OnValidatePrincipal to allow the claims to be changed.</param>
        public static void ConfigureCookiesForExtraAuth(this IServiceCollection services, bool updateCookieOnChange)
        {

            if (updateCookieOnChange)
            {
                services.AddSingleton<IAuthChanges, AuthChanges>();

                var sp = services.BuildServiceProvider();
                var extraAuthContextOptions = sp.GetRequiredService<DbContextOptions<ExtraAuthorizeDbContext>>();
                var protectionProvider = sp.GetService<IDataProtectionProvider>(); //NOTE: This can be null, which turns off impersonation

                var authCookieValidate = new AuthCookieValidate(extraAuthContextOptions, protectionProvider);
                var authCookieSigningOut = new AuthCookieSigningOut();

                //see https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-2.1#cookie-settings
                services.ConfigureApplicationCookie(options =>
                {
                    options.Events.OnValidatePrincipal = authCookieValidate.ValidateAsync;
                    //This ensures the impersonation cookie is deleted when a user signs out
                    options.Events.OnSigningOut = authCookieSigningOut.SigningOutAsync;
                });
            }
            else
            {
                services.AddSingleton<IAuthChanges>(x => null); //This will turn off the checks in the ExtraAuthDbContext

                //Simple version - see https://korzh.com/blogs/net-tricks/aspnet-identity-store-user-data-in-claims
                services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>, AddPermissionsToUserClaims>();
            }
        }
    }
}