// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.
namespace CommonCache
{
    public interface ISimpleTimeCache
    {
        bool GivenTicksIsLowerThanCachedTicks(object cacheKey, string ticksToCompareString);
        bool GivenTicksIsLowerThanCachedTicks(object cacheKey, long ticksToCompare);
        bool GetValueOrUseDefault(object cacheKey, long defaultValue, out long cachedValue);
        void AddOrUpdate(object cacheKey, long cachedValue);
    }
}