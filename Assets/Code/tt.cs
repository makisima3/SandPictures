using UnityEngine;
using UnityEngine.UI;

namespace Code
{
    public class tt : MonoBehaviour
    {
        [SerializeField] private Transform parent;
        [SerializeField] private Transform prototype;
        [ContextMenu("tt")]
        private void DoIt()
        {

           var t = Instantiate(prototype);
           t.GetComponent<Image>().enabled = false;
           t.SetParent(parent);
        }
    }
}