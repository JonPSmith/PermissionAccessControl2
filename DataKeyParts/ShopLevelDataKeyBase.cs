// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace DataKeyParts
{
    public class ShopLevelDataKeyBase : IShopLevelDataKey
    {
        [Required] //This means SQL will throw an error if we don't fill it in
        [MaxLength(DataAuthConstants.HierarchicalKeySize)]
        public string DataKey { get; private set; }

        //This method is used to set the shop-level classes' DataKey - the TenantBase classes set the property directly. 
        public void SetShopLevelDataKey(string key)
        {
            if (key != null && !key.EndsWith("*"))
                //The shop key must end in "*" (if null then we assume something else will set the DataKey
                throw new ApplicationException("You tried to set a shop-level DataKey but your key didn't end with *");

            DataKey = key;
        }
    }
}