// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DataLayer.EfCode;
using DataLayer.MultiTenantClasses;
using PermissionParts;

[assembly: InternalsVisibleTo("Test")]

namespace ServiceLayer.SeedDemo.Internal
{
    /// <summary>
    /// This is a extension method that converts a given format of text data (see default companyDefinitions)
    /// in to a set of tenants with hierarchical links between them. Just used to demo tenant data for display or unit testing
    /// ONLY USED FOR DEMO and UNIT TESTING
    /// </summary>
    public static class HierarchicalSeeder
    {
        public static List<Company> AddCompanyAndChildrenInDatabase(this CompanyDbContext context, params string[] companyDefinitions)
        {
            if (!companyDefinitions.Any())
                companyDefinitions = new[]
                {
                    "4U Inc.|West Coast|San Fran|SF Dress4U, SF Tie4U, SF Shirt4U",
                    "4U Inc.|West Coast|LA|LA Dress4U, LA Tie4U, LA Shirt4U"
                };

            var companyDict = new Dictionary<string, Company>();
            var subGroupsDict = new Dictionary<int, List<SubGroup>>();
            foreach (var companyDefinition in companyDefinitions)
            {
                var hierarchyNames = companyDefinition.Split('|');
                if (!companyDict.ContainsKey(hierarchyNames[0]))
                {
                    companyDict[hierarchyNames[0]] = Company.AddTenantToDatabaseWithSaveChanges(
                        hierarchyNames[0], PaidForModules.None, context);
                    subGroupsDict.Clear();
                }

                TenantBase parent = companyDict[hierarchyNames[0]];

                for (int i = 1; i < hierarchyNames.Length; i++)
                {
                    if (!subGroupsDict.ContainsKey(i))
                    {
                        subGroupsDict[i] = new List<SubGroup>();
                    }
                    if (i + 1 == hierarchyNames.Length)
                    {
                        //End, which are shops
                        var shopNames = hierarchyNames[i].Split(',').Select(x => x.Trim());
                        foreach (var shopName in shopNames)
                        {
                            RetailOutlet.AddTenantToDatabaseWithSaveChanges(shopName, parent, context);
                        }
                    }
                    else
                    {
                        //Groups
                        SubGroup subGroup = null;
                        if (subGroupsDict[i].Any(x => x.Name == hierarchyNames[i]))
                        {
                            subGroup = subGroupsDict[i].Single(x => x.Name == hierarchyNames[i]);
                        }
                        else
                        {
                            subGroup = SubGroup.AddTenantToDatabaseWithSaveChanges(hierarchyNames[i], parent, context);
                            subGroupsDict[i].Add(subGroup);
                        }
                        parent = subGroup;
                    }
                }
            }

            return new List<Company>(companyDict.Values);
        }
    }
}