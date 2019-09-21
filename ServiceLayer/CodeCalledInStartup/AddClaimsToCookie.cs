// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using CommonCache;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.AuthCookieVersions;

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
        /// <param name="authCookieVersion">Controls the type of cookie validation used.</param>
        public static void ConfigureCookiesForExtraAuth(this IServiceCollection services, string authCookieVersion)
        {
            IAuthCookieValidate cookieEventMethod = null;
            switch (authCookieVersion)
            {
                case "Off":
                    //This turns the permissions/datakey totally off - you are only using ASP.NET Core 
                    break;
                case "None":
                    //This uses UserClaimsPrincipal to set the claims on login - easy and quick.
                    //Simple version - see https://korzh.com/blogs/net-tricks/aspnet-identity-store-user-data-in-claims
                    services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>, AddPermissionsToUserClaims>();
                    break;
                case "Basic":
                    cookieEventMethod = new AuthCookieValidateBasic();
                    break;
                default: 
                    throw new ArgumentException($"{authCookieVersion} isn't a valid version");
            }

            if (cookieEventMethod != null)
            {
                services.ConfigureApplicationCookie(options =>
                {
                    options.Events.OnValidatePrincipal = cookieEventMethod.ValidateAsync;
                });
            }

            if (authCookieVersion != "RefreshClaims")
            {
                services.AddSingleton<IAuthChanges>(x => null); //This will turn off the checks in the ExtraAuthDbContext
            }


            //if (updateCookieOnChange)
            //{
            //    services.AddSingleton<IAuthChanges, AuthChanges>();
            //    //User impersonation needs the encryption services provided by AddDataProtection
            //    services.AddDataProtection();

            //    var sp = services.BuildServiceProvider();
            //    var extraAuthContextOptions = sp.GetRequiredService<DbContextOptions<ExtraAuthorizeDbContext>>();
            //    var protectionProvider = sp.GetService<IDataProtectionProvider>(); //NOTE: This can be null, which turns off impersonation

            //    var authCookieValidate = new AuthCookieValidate(extraAuthContextOptions, protectionProvider);
            //    var authCookieSigningOut = new AuthCookieSigningOut();

            //    //see https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-2.1#cookie-settings
            //    services.ConfigureApplicationCookie(options =>
            //    {
            //        options.Events.OnValidatePrincipal = authCookieValidate.ValidateAsync;
            //        //This ensures the impersonation cookie is deleted when a user signs out
            //        options.Events.OnSigningOut = authCookieSigningOut.SigningOutAsync;
            //    });
            //}
            //else
            //{
            //    services.AddSingleton<IAuthChanges>(x => null); //This will turn off the checks in the ExtraAuthDbContext

            //    //Simple version - see https://korzh.com/blogs/net-tricks/aspnet-identity-store-user-data-in-claims
            //    services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>, AddPermissionsToUserClaims>();
            //}
        }
    }
}