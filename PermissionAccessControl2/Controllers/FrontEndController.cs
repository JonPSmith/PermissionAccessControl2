using System.Collections.Generic;
using System.Linq;
using FeatureAuthorize;
using Microsoft.AspNetCore.Mvc;
using PermissionParts;

namespace PermissionAccessControl2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrontEndController : ControllerBase
    {
        /// <summary>
        /// This returns null (HTTP 204 - no content) if no logged in user, otherwise a array of the logged in user's permissions
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            var packedPermissions = HttpContext.User?.Claims
                .SingleOrDefault(x => x.Type == PermissionConstants.PackedPermissionClaimType);
            return packedPermissions?.Value.UnpackPermissionsFromString().Select(x => x.ToString());
        }
    }
}
