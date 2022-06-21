using UnityEngine;

namespace Plugins.RobyyUtils
{
    /// <summary>
    /// Generic based singleton for MonoBehaviours.<br>
    /// </summary>
    /// <typeparam name="T">is interface of singleton</typeparam>
    public class Singleton<T> : MonoBehaviour 
        where T : class
    {
        /// <summary>
        /// Makes the object singleton not be destroyed automatically when loading a new scene.
        /// </summary>
        [SerializeField] private bool dontDestroy;

        private static T _instance;
        public static T Instance => _instance;

        protected virtual void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this as T;
                if (dontDestroy)
                    DontDestroyOnLoad(gameObject);
                Initialize();
            }
        }

        public static void Mock(T mock)
        {
            _instance = mock;
        }

        protected virtual void Initialize()
        {
        }

        protected virtual void Shutdown()
        {
        }

        protected virtual void OnDestroy()
        {
            if (_instance as Singleton<T> == this)
            {
                try
                {
                    Shutdown();
                }
                finally
                {
                    _instance = null;
                }
            }
        }
    }
}