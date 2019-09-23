using System.ComponentModel.DataAnnotations;
using DataKeyParts;
using RefreshClaimsParts;

namespace DataLayer.ExtraAuthClasses
{
    [NoQueryFilterNeeded]
    public class TimeStore
    {
        [Key]
        [Required]
        [MaxLength(AuthChangesConsts.CacheKeyMaxSize)]
        public string Key { get; set; }

        public long LastUpdatedTicks { get; set; }
    }
}