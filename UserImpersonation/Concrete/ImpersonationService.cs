// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using FeatureAuthorize;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;

namespace UserImpersonation.Concrete
{
    public class ImpersonationService : IImpersonationService
    {
        private readonly HttpContext _httpContext;
        private readonly ImpersonationCookie _cookie;

        public ImpersonationService(IHttpContextAccessor httpContextAccessor, IDataProtectionProvider protectionProvider)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _cookie = protectionProvider != null //If protectionProvider is null then impersonation is turned off
                    ? new ImpersonationCookie(_httpContext, protectionProvider)
                    : null;
        }

        /// <summary>
        /// This creates an user impersonation cookie, which starts the user impersonation via the AuthCookie ValidateAsync event
        /// </summary>
        /// <param name="userId">This must be the userId of the user you want to impersonate</param>
        /// <param name="userName"></param>
        /// <param name="keepOwnPermissions"></param>
        /// <returns>Error message, or null if OK.</returns>
        public string StartImpersonation(string userId, string userName, bool keepOwnPermissions)
        {
            if (_cookie == null)
                return "Impersonation is turned off in this application.";
            if (!_httpContext.User.Identity.IsAuthenticated)
                return "You must be logged in to impersonate a user.";
            if (_httpContext.User.Claims.GetUserIdFromClaims() == userId)
                return "You cannot impersonate yourself.";
            if (_httpContext.User.InImpersonationMode())
                return "You are already in impersonation mode.";
            if (userId == null)
                return "You must provide a userId string";
            if (userName == null)
                return "You must provide a username string";

            _cookie.AddUpdateCookie(new ImpersonationData(userId, userName, keepOwnPermissions).GetPackImpersonationData());
            return null;
        }

        /// <summary>
        /// This will delete the user impersonation cookie, which causes the AuthCookie ValidateAsync event to revert to the original user
        /// </summary>
        /// <returns>error message, or null if OK</returns>
        public string StopImpersonation()
        {
            if (!_httpContext.User.InImpersonationMode())
                return "You aren't in impersonation mode.";

            _cookie.Delete();
            return null;
        }
    }
}