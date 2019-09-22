// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace DataKeyParts
{
    /// <summary>
    /// This is used to mark a database class that doesn't need a query filter.
    /// This is only there so that you can unit test that all filters are set up for the classes that do have an access key
    /// </summary>
    public class NoQueryFilterNeeded : Attribute
    {
        
    }
}