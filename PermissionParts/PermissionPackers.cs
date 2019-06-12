// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PermissionParts
{
    public static class PermissionPackers
    {
        public static string PackPermissionsIntoString(this IEnumerable<Permissions> permissions)
        {
            return permissions.Aggregate("", (s, permission) => s + (char)permission);
        }

        public static IEnumerable<Permissions> UnpackPermissionsFromString(this string packedPermissions)
        {
            if (packedPermissions == null)
                throw new ArgumentNullException(nameof(packedPermissions));
            foreach (var character in packedPermissions)
            {
                yield return ((Permissions) character);
            }
        }

        public static Permissions? FindPermissionViaName(this string permissionName)
        {
            return Enum.TryParse(permissionName, out Permissions permission)
                ? (Permissions?) permission
                : null;
        }

    }
}