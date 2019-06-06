// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataAuthorize;

namespace DataLayer.AppClasses
{
    /// <summary>
    /// This class is marked with the DoesNotHaveAccessKey attribute to tell the unit tests it doesn't need a query filter
    /// </summary>
    [DoesNotHaveAccessKey]
    public class AnyoneCanAccess
    {
        public int Id { get; set; }
        public string GeneralData { get; set; }
    }
}