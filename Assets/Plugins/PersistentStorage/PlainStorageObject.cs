using System;
using Newtonsoft.Json;

namespace PersistentStorage
{
    public abstract class PlainStorageObject<TData> : IStorageObject<TData>
    {
        public abstract string PrefKey { get; }
        public virtual TData Data { get; set; }
        
        private Action<TData> _afterLoading;
        private Func<TData> _beforeSaving;
        private readonly TData _defaultData;

        public PlainStorageObject(TData defaultData, Action<TData> afterLoading = null, Func<TData> beforeSaving = null)
        {
            _defaultData = defaultData;
            Set(afterLoading, beforeSaving);
        }
        
        public void Set(Action<TData> afterLoading = null, Func<TData> beforeSaving = null)
        {
            if (afterLoading != null)
                _afterLoading = afterLoading;
            
            if(beforeSaving != null)
                _beforeSaving = beforeSaving;
        }

        public void LoadDefaults()
        {
            var json = JsonConvert.SerializeObject(_defaultData);
            Data = JsonConvert.DeserializeObject<TData>(json);
        }
        
        public void AfterLoading() => _afterLoading?.Invoke(Data);

        public void BeforeSaving()
        {
            if (_beforeSaving == null)
                return;
            
            Data = _beforeSaving();
        }
    }
}