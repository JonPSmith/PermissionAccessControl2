// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.EfCode;

namespace DataAuthorize
{
    public class CalcDataKey
    {
        private readonly ExtraAuthorizeDbContext _context;

        public CalcDataKey(ExtraAuthorizeDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// This looks for a DataKey for the current user, which can be missing
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>The found data key, or random guid string to stop it matching anything</returns>
        public string CalcDataKeyForUser(string userId)
        {
            return _context.DataAccess.Where(x => x.UserId == userId)
                .Select(x => x.LinkedTenant.DataKey).SingleOrDefault() 
                   //If no data key then set to random guid to stop it matching anything
                   ?? Guid.NewGuid().ToString("N");
        }
    }
}