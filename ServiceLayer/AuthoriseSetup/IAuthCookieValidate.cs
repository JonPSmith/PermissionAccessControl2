// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ServiceLayer.AuthoriseSetup
{
    public interface IAuthCookieValidate
    {
        Task ValidateAsync(CookieValidatePrincipalContext context);
    }
}