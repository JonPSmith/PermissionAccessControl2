// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.ExtraAuthClasses
{
    /// <summary>
    /// This is a one-to-many relationship between the User (represented by the UserId) and their Roles (represented by RoleToPermissions)
    /// </summary>
    public class UserToRole
    {
        //I use a composite key for this table: combination of UserId and RoleName 
        [Key]
        [Column(Order = 1)]
        [Required(AllowEmptyStrings = false)]
        [MaxLength(ExtraAuthConstants.UserIdSize)] 
        public string UserId { get; private set; }

        [Key]
        [Column(Order = 2)]
        [Required(AllowEmptyStrings = false)]
        [MaxLength(ExtraAuthConstants.RoleNameSize)]
        public string RoleName { get; private set; }

        [ForeignKey(nameof(RoleName))]
        public RoleToPermissions Role { get; private set; }

    }
}