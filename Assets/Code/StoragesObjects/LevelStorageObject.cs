using System;
using PersistentStorage;

namespace Code.StoragesObjects
{
    public class LevelStorageObject : PlainStorageObject<LevelStorageObject.LevelData>
    {
        [Serializable]
        public class LevelData
        {
            public int Level { get; set; }
        }

        public LevelStorageObject(LevelData defaultData, Action<LevelData> afterLoading = null, Func<LevelData> beforeSaving = null) : base(defaultData, afterLoading, beforeSaving)
        {
        }

        public override string PrefKey { get; }
    }
}