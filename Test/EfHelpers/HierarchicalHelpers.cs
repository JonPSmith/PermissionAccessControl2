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
        public static List<string> DisplayHierarchy(this TenantBase rootCompany)
        {
            var shopsStrings = new List<string>();
            foreach (var tenant in rootCompany.Children ?? new List<TenantBase>())
            {
                if (tenant is RetailOutlet)
                {
                    string shopDisplay = $"{tenant.Name}, DataKey = {tenant.DataKey ?? "<null>"}";
                    var parent = tenant.Parent;
                    while (parent != null)
                    {
                        shopDisplay = $"{parent.Name}->{shopDisplay}";
                        parent = parent.Parent;
                    }

                    shopsStrings.Add(shopDisplay);
                }
                else
                {
                    shopsStrings.AddRange(DisplayHierarchy(tenant));
                }
            }

            return shopsStrings;
        }

        public static Company AddCompanyAndChildrenInDatabase(this AppDbContext context, params string[] companyDefinitions)
        {
            var rootCompany = context.CreateCompanyAndChildren(companyDefinitions);
            var service = new TenantService(context);
            service.SetupCompany(rootCompany);
            return rootCompany;
        }

        public static Company CreateCompanyAndChildren(this AppDbContext context, params string[] companyDefinitions)
        {
            if (!companyDefinitions.Any())
                companyDefinitions = new[]
                {
                    "4U Inc.|West Coast|San Fran|SF Dress4U, SF Tie4U, SF Shirt4U",
                    "4U Inc.|West Coast|LA|LA Dress4U, LA Tie4U, LA Shirt4U"
                };

            Company rootCompany = null;
            foreach (var companyDefinition in companyDefinitions)
            {
                var hierarchyNames = companyDefinition.Split('|');
                if (rootCompany == null)
                    rootCompany = new Company(hierarchyNames[0], PaidForModules.None);

                TenantBase parent = rootCompany;
                for (int i = 1; i < hierarchyNames.Length; i++)
                {
                    if (i + 1 == hierarchyNames.Length)
                    {
                        //End, which are shops
                        var shopNames = hierarchyNames[i].Split(',').Select(x => x.Trim());
                        foreach (var shopName in shopNames)
                        {
                            parent.AddChild(new RetailOutlet(shopName, parent), context);
                        }
                    }
                    else
                    {
                        //Groups
                        var group = new Group(hierarchyNames[i], parent);
                        parent.AddChild(group, context);
                        parent = group;
                    }
                }
            }

            return rootCompany;
        }
    }
}