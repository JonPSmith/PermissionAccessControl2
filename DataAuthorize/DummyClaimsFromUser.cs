// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.


namespace DataAuthorize
{
    public class DummyClaimsFromUser : IGetClaimsProvider
    {
        public string UserId => null;
        public int ShopKey => 0;
        public string DistrictManagerId => null;
    }
}