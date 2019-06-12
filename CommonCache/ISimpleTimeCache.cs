// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.
namespace CommonCache
{
    public interface ISimpleTimeCache
    {
        bool GivenTicksIsHigherThanCachedTicks(object cacheKey, string ticksToCompareString);
        bool GivenTicksIsHigherThanCachedTicks(object cacheKey, long ticksToCompare);
        bool GetValueOrUseDefault(object cacheKey, int defaultValue, out int cachedValue);
        void AddOrUpdate(object cacheKey, int cachedValue);
    }
}