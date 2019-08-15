// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Internal;

[assembly: InternalsVisibleTo("Test")]

namespace ServiceLayer.UserImpersonation.Concrete.Internal
{
    internal class ImpersonationHandler
    {
        private const string ImpersonationClaimType = "Impersonalising";

        private readonly HttpContext _httpContext;
        private readonly IDataProtectionProvider _protectionProvider;
        private readonly ImpersonationCookie _cookie;
        private readonly List<Claim> _originalClaims;

        private readonly ImpersonationStates _impersonationState;

        public bool ImpersonationChange => _impersonationState != ImpersonationStates.NoChange;

        /// <summary>
        /// Creates ImpersonationHandler. NOTE: if protectionProvider is null then impersonation is turned off
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="protectionProvider"></param>
        /// <param name="originalClaims"></param>
        public ImpersonationHandler(HttpContext httpContext, IDataProtectionProvider protectionProvider, List<Claim> originalClaims)
        {
            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            _protectionProvider = protectionProvider;
            _cookie = new ImpersonationCookie();
            _originalClaims = originalClaims;

            _impersonationState = GetImpersonationState();
        }

        public string GetUserIdForWorkingOutPermissions()
        {
            if (_impersonationState == ImpersonationStates.Starting)
            {
                return _cookie.GetCookieInValue(_protectionProvider)
            }
        }

        //-------------------------------------------------
        //private methods

        private ImpersonationStates GetImpersonationState()
        {
            //If you set _protectionProvider to null it turns off the impersonation feature
            if (_protectionProvider == null)
                return ImpersonationStates.NoChange;

            var impCookieExists = _cookie.Exists(_httpContext.Request.Cookies);
            var impClaimExists = _originalClaims.Any(x => x.Type == ImpersonationClaimType);

            if (impClaimExists != impCookieExists)
            {
                return impClaimExists ? ImpersonationStates.Stopping : ImpersonationStates.Starting;
            }
            return ImpersonationStates.NoChange;
        }


    }
}