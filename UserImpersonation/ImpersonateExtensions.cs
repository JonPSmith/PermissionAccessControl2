// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Html;
using UserImpersonation.Concrete;

namespace UserImpersonation
{
    public static class ImpersonateExtensions
    {
        public static bool InImpersonationMode(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.Claims.Any(x => x.Type == ImpersonationHandler.ImpersonationClaimType);
        }

        public static string GetImpersonatedUserNameMode(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.Claims.SingleOrDefault(x => x.Type == ImpersonationHandler.ImpersonationClaimType)?.Value;
        }

        public static HtmlString GetCurrentUserNameAsHtml(this ClaimsPrincipal claimsPrincipal)
        {
            var impersonalisedName = claimsPrincipal.GetImpersonatedUserNameMode();
            var nameToShow = impersonalisedName ??
                             claimsPrincipal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Name)?.Value ??
                             "not logged in";

            return new HtmlString(
                "<span" + (impersonalisedName != null ? " class=\"text-danger\">Impersonating " : ">Hello ")
                     + $"{nameToShow}</span>");
        }
    }
}