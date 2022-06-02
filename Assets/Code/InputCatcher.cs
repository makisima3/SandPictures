using Code.InitDatas;
using Code.Levels;
using Plugins.SimpleFactory;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code
{
    public class InputCatcher : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IInitialized<InputCatcherInitData>
    {
        private Level _level;
        
        public void Initialize(InputCatcherInitData initData)
        {
            _level = initData.Level;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            _level.StartSpawn();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _level.StopSpawn();
        }

    }
}