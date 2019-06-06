// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using DataAuthorize;

namespace DataLayer.ExtraAuthClasses
{
    /// <summary>
    /// This handles the data access authorization
    /// </summary>
    public class UserDataAccess
    {
        public UserDataAccess(string userId, string accessKey)
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            AccessKey = accessKey;
        }

        [Key]
        [Required(AllowEmptyStrings = false)]
        [MaxLength(ExtraAuthConstants.UserIdSize)]
        public string UserId { get; private set; }

        [MaxLength(DataAuthConstants.AccessKeySize)]
        public string AccessKey { get; private set; }
    }
}