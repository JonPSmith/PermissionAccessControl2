// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.ExtraAuthClasses.Support
{
    public static class ChangeExtensions
    {
        public static bool UserPermissionsMayHaveChanged(this DbContext context)
        {
            return context.ChangeTracker.Entries()
                .Any(x => (x.Entity is IChangeEffectsUser && x.State == EntityState.Modified) || 
                          (x.Entity is IAddRemoveEffectsUser && 
                                (x.State == EntityState.Added || x.State == EntityState.Deleted)));
        }
    }
}