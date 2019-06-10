// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using DataAuthorize;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Test.EfHelpers
{
    public static class CheckEntitiesHelpers
    {
        public static IEnumerable<string> CheckEntitiesHasAQueryFilter(this IEnumerable<IEntityType> entityTypes)
        {
            foreach (var entityType in entityTypes)
            {
                if (entityType.QueryFilter == null
                    && entityType.BaseType == null //not a TPH subclass
                    && entityType.ClrType.GetCustomAttribute<OwnedAttribute>() == null //not an owned type
                    && entityType.ClrType.GetCustomAttribute<NoQueryFilterNeeded>() == null) //Not marked as global
                    yield return $"The entity class {entityType.Name} does not have a query filter";
            }
        }
    }
}