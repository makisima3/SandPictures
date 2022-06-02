namespace PersistentStorage
{
    public interface IStorageObject<TData>
    {
        string PrefKey { get; }
        TData Data { get; set; }
        
        void LoadDefaults();
        void AfterLoading();
        void BeforeSaving();
    }
}