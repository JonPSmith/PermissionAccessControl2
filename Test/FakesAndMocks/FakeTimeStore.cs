using CommonCache;

namespace Test.FakesAndMocks
{
    public class FakeTimeStore : ITimeStore
    {
        public FakeTimeStore(string key, long? value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; private set; }
        public long? Value { get; private set; }

        public long? GetValueFromStore(string key)
        {
            Key = key;
            return Value;
        }

        public void AddUpdateValue(string key, long value)
        {
            Key = key;
            Value = value;
        }
    }
}