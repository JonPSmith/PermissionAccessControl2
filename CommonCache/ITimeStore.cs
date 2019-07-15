using Microsoft.EntityFrameworkCore;

namespace CommonCache
{
    public interface ITimeStore
    {
        byte[] GetValueFromStore(string key);
        void AddUpdateValue(string key, byte[] value);
    }
}