using CommonCache;

namespace Test.FakesAndMocks
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