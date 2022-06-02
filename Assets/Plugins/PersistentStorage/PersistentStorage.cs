using Newtonsoft.Json;
using UnityEngine;

namespace PersistentStorage
{
    public static class PersistentStorage
    {
        public static TStorageObject Load<TStorageObject, TData>(TStorageObject storageObject)
            where TStorageObject : IStorageObject<TData>
        {
            if (PlayerPrefs.HasKey(storageObject.PrefKey))
            {
                var json = PlayerPrefs.GetString(storageObject.PrefKey);
                storageObject.Data = JsonConvert.DeserializeObject<TData>(json);
            }
            else
            {
                storageObject.LoadDefaults();
            }
            storageObject.AfterLoading();
            return storageObject;
        }

        public static TStorageObject Save<TStorageObject, TData>(TStorageObject storageObject)
            where TStorageObject : IStorageObject<TData>
        {
            storageObject.BeforeSaving();
            var json = JsonConvert.SerializeObject(storageObject.Data);
            PlayerPrefs.SetString(storageObject.PrefKey, json);
            return storageObject;
        }
    }
}