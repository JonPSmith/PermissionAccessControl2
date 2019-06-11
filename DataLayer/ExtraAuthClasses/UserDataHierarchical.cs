// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using DataLayer.AppClasses.MultiTenantParts;

namespace DataLayer.ExtraAuthClasses
{
    /// <summary>
    /// This handles a link to a hierarchical class that holds the required DataKey
    /// </summary>
    public class UserDataHierarchical : UserDataAccessBase 
    {
        public UserDataHierarchical(string userId, int linkedTenantId) : base(userId)
        {
            LinkedTenantId = linkedTenantId;
        }

        /// <summary>
        /// This holds the primary key of the Tenant the user is linked to
        /// </summary>
        public int LinkedTenantId { get; private set; }
    }
}