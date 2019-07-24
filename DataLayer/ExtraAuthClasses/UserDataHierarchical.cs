// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using DataLayer.MultiTenantClasses;

namespace DataLayer.ExtraAuthClasses
{
    /// <summary>
    /// This handles a link to a hierarchical class that holds the required DataKey
    /// </summary>
    public class UserDataHierarchical : UserDataAccessBase 
    {
        //This is needed by EF Core
        private UserDataHierarchical(string userId, int linkedTenantId) : base(userId)
        {
            LinkedTenantId = linkedTenantId;
        }

        public UserDataHierarchical(string userId, TenantBase linkedTenant) : base(userId)
        {
            Update(linkedTenant);
        }

        public void Update(TenantBase linkedTenant)
        {
            if (linkedTenant.TenantItemId == 0)
                throw new ApplicationException("The linkedTenant must be already in the database.");
            LinkedTenant = linkedTenant;
        }

        /// <summary>
        /// This holds the primary key of the Tenant the user is linked to
        /// </summary>
        public int LinkedTenantId { get; private set; }

        [ForeignKey(nameof(LinkedTenantId))]
        public TenantBase LinkedTenant { get; private set; }
    }
}