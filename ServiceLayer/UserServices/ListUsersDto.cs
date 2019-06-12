// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace ServiceLayer.UserServices
{
    public class ListUsersDto
    {
        public ListUsersDto(string email, string roleNames, string companyName, string linkedTenantName)
        {
            Email = email ?? throw new ArgumentNullException(nameof(email));
            RoleNames = roleNames ?? throw new ArgumentNullException(nameof(roleNames));
            CompanyName = companyName;
            LinkedTenantName = linkedTenantName ?? throw new ArgumentNullException(nameof(linkedTenantName));
        }

        public string Email { get; set; }
        public string RoleNames { get; set; }
        public string CompanyName { get; set; }
        public string LinkedTenantName { get; set; }
    }
}