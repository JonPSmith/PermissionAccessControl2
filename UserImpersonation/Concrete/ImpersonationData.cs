// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace UserImpersonation.Concrete
{
    /// <summary>
    /// This holds the data that will be put into the ImpersonationCookie
    /// </summary>
    public class ImpersonationData
    {
        /// <summary>
        /// UserId of the user you are impersonating
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// UserName of the impersonated user - used to show who you are impersonating
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// If true then when impersonating another user you will keep your original permissions
        /// </summary>
        public bool KeepOwnPermissions { get; }

        public ImpersonationData(string userId, string userName, bool keepOwnPermissions)
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            KeepOwnPermissions = keepOwnPermissions;
        }

        public ImpersonationData(string packedString)
        {
            var split = packedString.Split(',');
            if (split.Length != 3)
                throw new ArgumentException("The string didn't unpack to three items");

            UserId = split[0];
            KeepOwnPermissions = bool.Parse(split[1]);
            UserName = split[2];
        }
        
        public string GetPackImpersonationData()
        {
            return $"{UserId},{KeepOwnPermissions},{UserName}";
        }
    }
}