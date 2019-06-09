// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace DataAuthorize
{
    public class DataKeyBase : IDataKey
    {
        [Required] //This means SQL will throw an error if we don't fill it in
        [MaxLength(DataAuthConstants.HierarchicalKeySize)]
        public string DataKey { get; private set; }

        public void SetDataKey(string key)
        {
            DataKey = key;
        }
    }
}