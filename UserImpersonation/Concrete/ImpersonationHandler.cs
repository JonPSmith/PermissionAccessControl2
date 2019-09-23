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

namespace UserImpersonation.Concrete
{
    public class ImpersonationHandler
    {
        public const string ImpersonationClaimType = "Impersonalising";

        private enum ImpersonationStates
        {
            Normal,
            Starting,           //causes a recalc in AuthCookieValidate
            Impersonating,
            Stopping            //causes a recalc in AuthCookieValidate
        }

        private readonly HttpContext _httpContext;
        private readonly IDataProtectionProvider _protectionProvider;
        private readonly ImpersonationCookie _cookie;
        private readonly List<Claim> _originalClaims;

        private readonly Lazy<ImpersonationData> _startData;
        private readonly ImpersonationStates _impersonationState;

        /// <summary>
        /// If true then the Permissions/DataKey need recalculating due to impersonation starting/stopping
        /// </summary>
        public bool ImpersonationChange => _impersonationState == ImpersonationStates.Starting || 
                                           _impersonationState == ImpersonationStates.Stopping;

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
            //I use a lazy access to the cookie value as this takes a (bit) more time
            _startData = new Lazy<ImpersonationData>(() => new ImpersonationData(_cookie.GetCookieInValue()));

        }

        public string GetUserIdForWorkingOutPermissions()
        {
            return GetUserIdBasedOnRequirements(() => _startData.Value.KeepOwnPermissions);
        }

        public string GetUserIdForWorkingDataKey()
        {
            return GetUserIdBasedOnRequirements(() => false);
        }

        public void AddOrRemoveImpersonationClaim(List<Claim> claimsToGoIntoNewPrincipal)
        {
            switch (_impersonationState)
            {
                case ImpersonationStates.Normal:
                case ImpersonationStates.Impersonating:
                    break; //Do nothing
                case ImpersonationStates.Starting:
                    claimsToGoIntoNewPrincipal.Add(new Claim(ImpersonationClaimType, _startData.Value?.UserName));
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

        /// <summary>
        /// This returns the impersonated user's UserId if we are impersonating and the keepOwnPermissions is false
        /// </summary>
        /// <param name="keepOwnPermissionsFunc"></param>
        /// <returns></returns>
        private string GetUserIdBasedOnRequirements(Func<bool> keepOwnPermissionsFunc)
        {
            if ((_impersonationState == ImpersonationStates.Starting 
                 || _impersonationState == ImpersonationStates.Impersonating) 
                && keepOwnPermissionsFunc() == false)
            {
                return _startData.Value.UserId;
            }
            return _originalClaims.GetUserIdFromClaims();
        }

        private ImpersonationStates GetImpersonationState()
        {
            //If you set _protectionProvider to null it turns off the impersonation feature
            if (_protectionProvider == null)
                return ImpersonationStates.Normal;

            var impCookieExists = _cookie.Exists(_httpContext.Request.Cookies);
            var impClaimExists = _originalClaims.Any(x => x.Type == ImpersonationClaimType);

            if (impClaimExists != impCookieExists)
            {
                return impClaimExists ? ImpersonationStates.Stopping : ImpersonationStates.Starting;
            }
            return impCookieExists ? ImpersonationStates.Impersonating : ImpersonationStates.Normal;
        }
    }
}