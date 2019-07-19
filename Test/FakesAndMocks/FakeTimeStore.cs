using CommonCache;

namespace Test.FakesAndMocks
{
    public class FakeTimeStore : ITimeStore
    {
        public byte[] Value { get; set; }
        public string Key { get; set; }

        public byte[] GetValueFromStore(string key)
        {
            Key = key;
            return Value;
        }

        public void AddUpdateValue(string key, byte[] value)
        {
            Key = key;
            Value = value;
        }
    }
}