// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace DataAuthorize
{
    public class HierarchicalKeyBase : IHierarchicalKey
    {
        [Required] //This means SQL will throw an error if we don't fill it in
        [MaxLength(DataAuthConstants.HierarchicalKeySize)]
        public string DataKey { get; private set; }

        public void SetHierarchicalDataKey(string key)
        {
            if (key?.EndsWith("*") == true)
                //We only set the DataKey if the key is not null and ends in a "*"
                //This stops bad keys being used due to software errors (data won't be found)
                DataKey = key;
        }
    }
}