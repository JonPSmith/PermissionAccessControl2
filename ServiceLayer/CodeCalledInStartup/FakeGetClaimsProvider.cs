// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataAuthorize;

namespace ServiceLayer.CodeCalledInStartup
{
    public class FakeGetClaimsProvider : IGetClaimsProvider
    {
        public FakeGetClaimsProvider(string dataKey)
        {
            DataKey = dataKey;
        }

        public string DataKey { get; }
    }
}