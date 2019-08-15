// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Security.Claims;
using ServiceLayer.UserImpersonation.Concrete.Internal;

namespace ServiceLayer.UserImpersonation
{
    public static class ImpersonateExtensions
    {
        public static bool InImpersonationMode(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.Claims.Any(x => x.Type == ImpersonationHandler.ImpersonationClaimType);
        }
    }
}