// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using FeatureAuthorize;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;

[assembly: InternalsVisibleTo("Test")]

namespace ServiceLayer.UserImpersonation.Concrete.Internal
{
    internal class ImpersonationHandler
    {
        public const string ImpersonationClaimType = "Impersonalising";

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
            _cookie = new ImpersonationCookie(httpContext, protectionProvider);
            _originalClaims = originalClaims;

            _impersonationState = GetImpersonationState();
        }

        public string GetUserIdForWorkingOutPermissions()
        {
            if (_impersonationState == ImpersonationStates.Starting)
            {
                return _cookie.GetCookieInValue();
            }
            return _originalClaims.GetUserIdFromClaims();
        }

        public string GetUserIdForWorkingDataKey()
        {
            return GetUserIdForWorkingOutPermissions();
        }

        public void AddOrRemoveImpersonationClaim(List<Claim> claimsToGoIntoNewPrincipal)
        {
            switch (_impersonationState)
            {
                case ImpersonationStates.NoChange:
                    break; //Do nothing
                case ImpersonationStates.Starting:
                    claimsToGoIntoNewPrincipal.Add(new Claim(ImpersonationClaimType, ""));
                    break;
                case ImpersonationStates.Stopping:
                    var foundClaim = claimsToGoIntoNewPrincipal.Single(x => x.Type == ImpersonationClaimType);
                    claimsToGoIntoNewPrincipal.Remove(foundClaim);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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