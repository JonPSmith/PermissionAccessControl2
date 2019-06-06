// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;

namespace PermissionParts
{
    public static class PermissionChecker
    {
        public static bool ThisPermissionIsAllowed(this string packedPermissions, string permissionName)
        {
            var usersPermissions = packedPermissions.UnpackPermissionsFromString().ToArray();

            if (!Enum.TryParse(permissionName, true, out Permissions permissionToCheck))
                throw new InvalidEnumArgumentException($"{permissionName} could not be converted to a {nameof(Permissions)}.");

            return usersPermissions.Contains(permissionToCheck);
        }
    }
}