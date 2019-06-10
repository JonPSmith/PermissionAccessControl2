// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using DataLayer.AppClasses.MultiTenantParts;
using DataLayer.EfCode;
using PermissionParts;
using ServiceLayer.MultiTenant.Concrete;

namespace Test.EfHelpers
{
    public static class HierarchicalHelpers
    {

        public static Company AddCompanyAndChildrenInDatabase(this AppDbContext context, params string[] companyDefinitions)
        {
            var rootCompany = context.CreateCompanyAndChildren(companyDefinitions);
            var service = new TenantService(context);
            service.SetupCompany(rootCompany);
            return rootCompany;
        }

        private static Company CreateCompanyAndChildren(this AppDbContext context, params string[] companyDefinitions)
        {
            if (!companyDefinitions.Any())
                companyDefinitions = new[]
                {
                    "4U Inc.|West Coast|San Fran|SF Dress4U, SF Tie4U, SF Shirt4U",
                    "4U Inc.|West Coast|LA|LA Dress4U, LA Tie4U, LA Shirt4U"
                };

            Company rootCompany = null;
            var subGroupsDict = new Dictionary<int, List<SubGroup>>();
            foreach (var companyDefinition in companyDefinitions)
            {
                var hierarchyNames = companyDefinition.Split('|');
                if (rootCompany == null)
                    rootCompany = new Company(hierarchyNames[0], PaidForModules.None);

                TenantBase parent = rootCompany;

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
                            parent.Children.Add(new RetailOutlet(shopName, parent));
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
                            subGroup = new SubGroup(hierarchyNames[i], parent);
                            subGroupsDict[i].Add(subGroup);
                            parent.Children.Add(subGroup);
                        }
                        parent = subGroup;
                    }
                }
            }

            return rootCompany;
        }
    }
}