// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using DataAuthorize;

namespace DataLayer.AppClasses
{
    public class ShopDefinition : ITenantKey
    {
        /// <summary>
        /// The access key is its primary key
        /// </summary>
        [Key]
        [Required] //This means SQL will throw an error if we don't fill it in
        [MaxLength(DataAuthConstants.AccessKeySize)]
        public string DataKey { get; private set; }

        public string ShopName { get; private set; }

        public void SetAccessKey(string tenantKey)
        {
            DataKey = tenantKey;
        }
    }
}