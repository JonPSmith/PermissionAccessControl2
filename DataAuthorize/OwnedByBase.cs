// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace DataAuthorize
{
    public class OwnedByBase : IOwnedBy
    {
        [Required] //This means SQL will throw an error if we doing fill it in
        [MaxLength(40)] //A guid string is 36 characters long
        public string OwnedBy { get; private set; }

        public void SetOwnedBy(string protectKey)
        {
            OwnedBy = protectKey;
        }
    }
}