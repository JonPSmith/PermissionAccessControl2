// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace CommonCache
{
    public interface IAuthChanges
    {
        bool IsLowerThan(string cacheKey, string ticksToCompareString);
        void AddOrUpdate(string cacheKey, long cachedValue);
    }
}