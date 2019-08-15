// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;

namespace ServiceLayer.UserImpersonation.Concrete
{
    public class ImpersonationService : IImpersonationService
    {
        private readonly HttpContext _httpContext;
        private readonly IDataProtectionProvider _protectionProvider;
        private readonly ImpersonationCookie _cookie;

        public ImpersonationService(HttpContext httpContext, IDataProtectionProvider protectionProvider)
        {
            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            _protectionProvider = protectionProvider ?? throw new ArgumentNullException(nameof(protectionProvider));


            _cookie = new ImpersonationCookie();
        }

        /// <summary>
        /// This creates an user impersonation cookie, which starts the user impersonation via the AuthCookie ValidateAsync event
        /// </summary>
        /// <param name="userId">This must be the userId of the user you want to impersonate</param>
        public void StartImpersonation(string userId)
        {
            _cookie.AddUpdateCookie(userId, _protectionProvider, _httpContext.Response.Cookies);
        }

        /// <summary>
        /// This will delete the user impersonation cookie, which causes the AuthCookie ValidateAsync event to revert to the original user
        /// </summary>
        public void StopImpersonation()
        {
            _cookie.Delete(_httpContext.Response.Cookies);
        }
    }
}