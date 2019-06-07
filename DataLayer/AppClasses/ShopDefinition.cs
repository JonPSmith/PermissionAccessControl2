// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using DataAuthorize;
using PermissionParts;

namespace DataLayer.AppClasses
{

    /// <summary>
    /// This contains the definition of a 
    /// </summary>
    [DoesNotNeedAccessKey]
    public class ShopDefinition
    {
        public ShopDefinition(string shopName, PaidForModules allowedPaidForModules)
        {
            ShopName = shopName ?? throw new ArgumentNullException(nameof(shopName));
            AllowedPaidForModules = allowedPaidForModules;
        }

        /// <summary>
        /// The shop's primary key becomes the Access Key
        /// </summary>
        [Key]
        public Guid ShopKey { get; private set; }

        [Required]
        public string ShopName { get; private set; }

        #region PaidForModules

        /// <summary>
        /// This holds the modules this multi-tenant user has access to
        /// </summary>
        public PaidForModules AllowedPaidForModules { get; set; }

        #endregion
    }
}