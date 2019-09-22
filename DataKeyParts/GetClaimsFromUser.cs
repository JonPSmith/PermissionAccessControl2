// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Http;

namespace DataKeyParts
{
    public class GetClaimsFromUser : IGetClaimsProvider
    {
        public GetClaimsFromUser(IHttpContextAccessor accessor)
        {
            DataKey = accessor.HttpContext?.User.Claims
                .SingleOrDefault(x => x.Type == DataAuthConstants.HierarchicalKeyClaimName)?.Value;
        }

        public string DataKey { get; }
    }
}