// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using DataLayer.AppClasses.MultiTenantParts;

namespace DataLayer.ExtraAuthClasses
{
    /// <summary>
    /// This handles a link to a hierarchical class that holds the required DataKey
    /// </summary>
    public class UserDataHierarchical<T> : UserDataAccessBase where T : TenantBase
    {
        public UserDataHierarchical(string userId, T tenant) : base(userId)
        {
            LinkedTenant = tenant;
        }

        public int LinkedTenantId { get; private set; }

        [ForeignKey(nameof(LinkedTenantId))]
        public T LinkedTenant { get; private set; }
    }
}