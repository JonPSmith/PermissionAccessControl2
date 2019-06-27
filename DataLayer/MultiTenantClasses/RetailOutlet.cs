// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace DataLayer.MultiTenantClasses
{
    public class RetailOutlet : TenantBase
    {
        private RetailOutlet(string name) : base(name) { } //Needed by EF Core

        public RetailOutlet(string name, TenantBase parent) : base(name, parent)
        {
        }
    }
}