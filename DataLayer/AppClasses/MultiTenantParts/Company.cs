// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using PermissionParts;

namespace DataLayer.AppClasses.MultiTenantParts
{
    /// <summary>
    /// This is the root entry for a multi-tenant entry. There should be only one Company per multi-tenan
    /// </summary>
    public class Company : TenantBase
    {
        public Company(PaidForModules allowedPaidForModules, string name) : base(name, null)
        {
            AllowedPaidForModules = allowedPaidForModules;
        }

        /// <summary>
        /// This holds the modules this company have purchased
        /// </summary>
        public PaidForModules AllowedPaidForModules { get; private set; }


    }
}