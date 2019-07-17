using CommonCache;
using Microsoft.Extensions.Caching.Distributed;

namespace Test.EfHelpers
{
    public class FakeAuthChangesFactory : IAuthChangesFactory
    {
        public FakeAuthChanges FakeAuthChanges { get; } = new FakeAuthChanges();

        public IAuthChanges CreateIAuthChange(ITimeStore timeStore)
        {
            return FakeAuthChanges;
        }
    }
}