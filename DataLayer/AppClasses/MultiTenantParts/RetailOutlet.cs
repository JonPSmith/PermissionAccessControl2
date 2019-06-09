// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataAuthorize;

namespace DataLayer.AppClasses.MultiTenantParts
{
    public class RetailOutlet : TenantBase
    {
        public RetailOutlet(string name, TenantBase parent) : base(name, parent)
        {
        }
    }
}