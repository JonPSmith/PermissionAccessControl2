// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace DataLayer.AppClasses.MultiTenantParts
{
    public class SubGroup : TenantBase
    {
        private SubGroup(string name) : base(name) { } //Needed by EF Core

        public SubGroup(string name, TenantBase parent) : base(name, parent)
        {
        }
    }
}