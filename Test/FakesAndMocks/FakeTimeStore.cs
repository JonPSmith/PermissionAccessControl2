using CommonCache;

namespace Test.FakesAndMocks
{
    public class FakeTimeStore : ITimeStore
    {
        public long? Value { get; set; }
        public string Key { get; set; }

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