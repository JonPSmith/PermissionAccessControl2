using System.ComponentModel.DataAnnotations;
using CommonCache;
using DataAuthorize;

namespace DataLayer.ExtraAuthClasses
{
    [NoQueryFilterNeeded]
    public class TimeStore
    {
        [Key]
        [Required]
        [MaxLength(AuthChangesConsts.CacheKeyMaxSize)]
        public string Key { get; set; }

        [MaxLength(AuthChangesConsts.CacheValueMaxSize)]
        public byte[] Value { get; set; }
    }
}