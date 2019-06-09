// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DataAuthorize
{
    public class GetClaimsFromUser : IGetClaimsProvider
    {
        public GetClaimsFromUser(IHttpContextAccessor accessor)
        {
            UserId = accessor.HttpContext?.User.Claims
                .SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            DataKey = accessor.HttpContext?.User.Claims
                .SingleOrDefault(x => x.Type == DataAuthConstants.HierarchicalKeyClaimName)?.Value;
        }

        public string UserId { get; }
        public string DataKey { get; }
    }
}