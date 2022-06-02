namespace Plugins.SimpleFactory
{
    public interface IInitialized<in TInitData>
    {
        void Initialize(TInitData initData);
    }
}