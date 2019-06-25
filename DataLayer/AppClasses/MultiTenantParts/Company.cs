// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using DataAuthorize;
using PermissionParts;

namespace DataLayer.AppClasses.MultiTenantParts
{
    /// <summary>
    /// This is the root entry for a multi-tenant entry. There should be only one Company per multi-tenant
    /// </summary>
    public class Company : TenantBase
    {
        private Company(string name) : base(name) { } //Needed by EF Core

        public Company(string name, PaidForModules allowedPaidForModules) : base(name, null)
        {
            AllowedPaidForModules = allowedPaidForModules;
        }

        /// <summary>
        /// This holds the modules this company have purchased
        /// </summary>
        public PaidForModules AllowedPaidForModules { get; set; }
    }
}