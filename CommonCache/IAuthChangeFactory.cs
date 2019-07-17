namespace CommonCache
{
    public interface IAuthChangesFactory
    {
        IAuthChanges CreateIAuthChange(ITimeStore timeStore);
    }
}