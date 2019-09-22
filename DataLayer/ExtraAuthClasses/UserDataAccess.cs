// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using DataKeyParts;

namespace DataLayer.ExtraAuthClasses
{
    /// <summary>
    /// This handles the data access authorization
    /// </summary>
    public class UserDataAccessKey  : UserDataAccessBase
    {
        public UserDataAccessKey(string userId, string accessKey) : base(userId)
        {
            AccessKey = accessKey;
        }

        [MaxLength(DataAuthConstants.AccessKeySize)]
        public string AccessKey { get; private set; }
    }
}