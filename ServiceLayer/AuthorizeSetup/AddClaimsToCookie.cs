// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using CommonCache;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.UserImpersonation.Concrete.Internal;

namespace ServiceLayer.AuthorizeSetup
{
    public static class AddClaimsToCookie
    {
        /// <summary>
        /// This configures how the user's claims get updated with the Permissions/DataKey. 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="authCookieVersion">Controls the type of cookie validation used.</param>
        public static void ConfigureCookiesForExtraAuth(this IServiceCollection services, string authCookieVersion)
        {
            IAuthCookieValidate cookieEventClass = null;
            switch (authCookieVersion)
            {
                case "Off":
                    //This turns the permissions/datakey totally off - you are only using ASP.NET Core logged-in user 
                    break;
                case "None":
                    //This uses UserClaimsPrincipal to set the claims on login - easy and quick.
                    //Simple version - see https://korzh.com/blogs/net-tricks/aspnet-identity-store-user-data-in-claims
                    services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>, AddPermissionsToUserClaims>();
                    break;
                case "PermissionsOnly":
                    //Event - only permissions set up
                    cookieEventClass = new AuthCookieValidatePermissionsOnly();
                    break;
                case "PermissionsDataKey":
                     // Event - Permissions and DataKey set up
                     cookieEventClass = new AuthCookieValidatePermissionsDataKey();
                    break;
                case "RefreshClaims":
                    cookieEventClass = new AuthCookieValidateRefreshClaims();
                    break;
                case "Impersonation":
                case "EveryThing":
                    // Event - Permissions and DataKey set up, provides User Impersonation + possible "RefreshClaims"
                    services.AddDataProtection();   //DataProtection is needed to encrypt the data in the Impersonation cookie
                    var validateAsyncVersion = authCookieVersion == "Impersonation"
                        ? (IAuthCookieValidate)new AuthCookieValidateImpersonation()
                        : (IAuthCookieValidate)new AuthCookieValidateEverything();
                    //We set two events, so we do this here
                    services.ConfigureApplicationCookie(options =>
                    {
                        options.Events.OnValidatePrincipal = validateAsyncVersion.ValidateAsync;
                        //This ensures the impersonation cookie is deleted when a user signs out
                        options.Events.OnSigningOut = new AuthCookieSigningOut().SigningOutAsync;
                    });
                    break;
                default: 
                    throw new ArgumentException($"{authCookieVersion} isn't a valid version");
            }

            if (cookieEventClass != null)
            {
                services.ConfigureApplicationCookie(options =>
                {
                    options.Events.OnValidatePrincipal = cookieEventClass.ValidateAsync;
                });
            }

            if (authCookieVersion == "RefreshClaims" || authCookieVersion == "EveryThing")
            {
                services.AddSingleton<IAuthChanges, AuthChanges>();
            }
            else
            {
                services.AddSingleton<IAuthChanges>(x => null); //This will turn off the checks in the ExtraAuthDbContext
            }
        }
    }
}