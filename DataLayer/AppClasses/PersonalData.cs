// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataAuthorize;

namespace DataLayer.AppClasses
{
    public class PersonalData : DataKeyBase, IUserId
    {
        public int PersonalDataId { get; set; }

        public string YourNote { get; set; }
    }
}