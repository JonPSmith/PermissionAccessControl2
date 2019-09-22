// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.ExtraAuthClasses
{
    /// <summary>
    /// This handles the data access authorization
    /// </summary>
    public class UserDataAccessBase
    {
        public UserDataAccessBase(string userId)
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        }

        [Key]
        [Required(AllowEmptyStrings = false)]
        [MaxLength(ExtraAuthConstants.UserIdSize)]
        public string UserId { get; private set; }
    }
}