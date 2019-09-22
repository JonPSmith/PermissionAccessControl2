// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace DataKeyParts
{
    public interface IShopLevelDataKey : IDataKey
    {
        //This method is used to set the shop-level classes' DataKey - the TenantBase classes set the property directly. 
        void SetShopLevelDataKey(string tenantKey);
    }
}