// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using PermissionParts;

namespace DataLayer.ExtraAuthClasses
{
    /// <summary>
    /// This holds what modules a user or tenant has
    /// </summary>
    public class ModulesForUser
    {
        public ModulesForUser(string moduleKey, PaidForModules allowedPaidForModules)
        {
            ModuleKey = moduleKey ?? throw new ArgumentNullException(nameof(moduleKey));
            AllowedPaidForModules = allowedPaidForModules;
        }

        /// <summary>
        /// The ModuleKey can be either the UserId, or in multi-tenant systems it is likely to be the tenant Id
        /// </summary>
        [Key]
        [MaxLength(ExtraAuthConstants.ModuleKeySize)]
        public string ModuleKey { get; private set; }

        public PaidForModules AllowedPaidForModules { get; private set; }
    }
}