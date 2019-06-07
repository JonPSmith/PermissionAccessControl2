// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataAuthorize;

namespace Test.FakesAndMocks
{
    public class FakeGetClaimsProvider : IGetClaimsProvider
    {
        public FakeGetClaimsProvider(string userId, string accessKey)
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            AccessKey = accessKey;
        }

        public string AccessKey { get; }
        public string UserId { get; }
    }
}