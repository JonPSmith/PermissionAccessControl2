// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;

[assembly: InternalsVisibleTo("Test")]

namespace ServiceLayer.UserImpersonation.Concrete.Internal
{
    internal class ImpersonationCookie
    {
        private const string CookieName = "UserImpersonation";
        private readonly CookieOptions _options;

        public string EncryptPurpose { get; private set; }

        public ImpersonationCookie()
        {
            EncryptPurpose = "hffhegse432!&2!jbK!K3wqqqagg3bbassdewdsgfedgbfdewe13c";
            _options = new CookieOptions
            {
                Secure = false,  //In real life you would want this to be true, but for this demo I allow http
                HttpOnly = true, //Not used by JavaScript
                IsEssential = true,
                Expires = DateTime.MinValue //Make it a session cookie for more safely, i.e. it is deleted if the browser is closed
            };
        }

        public void AddUpdateCookie(string data, IDataProtectionProvider provider, IResponseCookies cookiesOut)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (cookiesOut == null) throw new ArgumentNullException(nameof(cookiesOut));

            var protector = provider.CreateProtector(EncryptPurpose);
            var encryptedString = protector.Protect(data);
            cookiesOut.Append(CookieName, encryptedString, _options);
        }

        public bool Exists(IRequestCookieCollection cookiesIn)
        {
            return cookiesIn[CookieName] != null;
        }

        public string GetCookieInValue(IDataProtectionProvider provider, IRequestCookieCollection cookiesIn,
            IResponseCookies cookiesOut)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (cookiesIn == null) throw new ArgumentNullException(nameof(cookiesIn));

            var cookieData = cookiesIn[CookieName];
            if (string.IsNullOrEmpty(cookieData))
                return null;

            var protector = provider.CreateProtector(EncryptPurpose);
            string decrypt = null;
            try
            {
                decrypt = protector.Unprotect(cookieData);
            }
            catch (Exception e)
            {
                //_logger.LogError(e, "Error decoding a cookie. Have deleted cookie to stop problem.");
                Delete(cookiesOut);
                throw;
            }

            return decrypt;
        }

        public void Delete(IResponseCookies cookiesOut)
        {
            cookiesOut.Delete(CookieName, _options);
        }
    }
}