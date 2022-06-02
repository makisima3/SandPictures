using System;
using Newtonsoft.Json;
using UnityEngine;

namespace PersistentStorage
{
    public abstract class MonoStorageObject<TData> : MonoBehaviour, IStorageObject<TData>
    {
        [SerializeField] private TData defaultData;

        public abstract string PrefKey { get; }
        
        public virtual TData Data { get; set; }

        private Action<TData> _afterLoading;
        private Func<TData> _beforeSaving;

        public void Set(Action<TData> afterLoading = null, Func<TData> beforeSaving = null)
        {
            if (afterLoading != null)
                _afterLoading = afterLoading;
            
            if(beforeSaving != null)
                _beforeSaving = beforeSaving;
        }

        public void LoadDefaults()
        {
            var json = JsonConvert.SerializeObject(defaultData);
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