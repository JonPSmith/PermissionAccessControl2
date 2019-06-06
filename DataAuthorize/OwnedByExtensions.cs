// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DataAuthorize
{
    public static class OwnedByExtensions
    {
        /// <summary>
        /// This is called in the overridden SaveChanges in the application's DbContext
        /// Its job is to call the SetOwnedBy on entities that have it and are being created
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userId"></param>
        public static void MarkCreatedItemAsOwnedBy(this DbContext context, string userId)
        {
            foreach (var entityEntry in context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added))
            {
                if (entityEntry.Entity is IOwnedBy entityToMark)
                {
                    entityToMark.SetOwnedBy(userId);
                }
            }
        }


        public static void MarkCreatedItemWithShopKey(this DbContext context, int shopKey)
        {
            //At startup shopKey will be zero, so ignore the setting of the ShopKey
            //This allows my seeding code to work
            if (shopKey == 0) return;

            foreach (var entityEntry in context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added))
            {
                if (entityEntry.Entity is IShopKey entityToMark)
                {
                    entityToMark.SetShopKey(shopKey);
                }
            }
        }
    }
}