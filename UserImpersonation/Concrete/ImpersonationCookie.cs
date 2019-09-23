// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;

[assembly: InternalsVisibleTo("Test")]

namespace UserImpersonation.Concrete
{
    public class ImpersonationCookie
    {
        private const string CookieName = "UserImpersonation";

        private readonly HttpContext _httpContext;
        private readonly IDataProtectionProvider _protectionProvider;
        private readonly CookieOptions _options;

        public string EncryptPurpose { get; private set; }

        public ImpersonationCookie(HttpContext httpContext, IDataProtectionProvider protectionProvider)
        {
            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            _protectionProvider = protectionProvider; //Can be null
            EncryptPurpose = "hffhegse432!&2!jbK!K3wqqqagg3bbassdewdsgfedgbfdewe13c";
            _options = new CookieOptions
            {
                Secure = false,  //In real life you would want this to be true, but for this demo I allow http
                HttpOnly = true, //Not used by JavaScript
                IsEssential = true,
                //These two make it a session cookie, i.e. it disappears when the browser is closed
                Expires = null,
                MaxAge = null
            };
        }

        public void AddUpdateCookie(string data)
        {
            if (_protectionProvider == null)
                throw new NullReferenceException(
                    $"The {nameof(IDataProtectionProvider)} was null, which means impersonation is turned off.");

            var protector = _protectionProvider.CreateProtector(EncryptPurpose);
            var encryptedString = protector.Protect(data);
            _httpContext.Response.Cookies.Append(CookieName, encryptedString, _options);
        }

        public bool Exists(IRequestCookieCollection cookiesIn)
        {
            return cookiesIn[CookieName] != null;
        }

        public string GetCookieInValue()
        {
            if (_protectionProvider == null)
                throw new NullReferenceException(
                    $"The {nameof(IDataProtectionProvider)} was null, which means impersonation is turned off.");

            var cookieData = _httpContext.Request.Cookies[CookieName];
            if (string.IsNullOrEmpty(cookieData))
                return null;

            var protector = _protectionProvider.CreateProtector(EncryptPurpose);
            string decrypt = null;
            try
            {
                decrypt = protector.Unprotect(cookieData);
            }
            catch (Exception e)
            {
                //_logger.LogError(e, "Error decoding a cookie. Have deleted cookie to stop any further problems.");
                Delete();
                throw;
            }

            return decrypt;
        }

        public void Delete()
        {
            _httpContext.Response.Cookies.Delete(CookieName, _options);
        }
    }
}