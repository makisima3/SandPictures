namespace CorgiFallingSands
{
    using UnityEngine;
    using UnityEngine.UI;

    public class ButtonScrollView : MonoBehaviour
    {
        public Scrollbar ourScrollbar;
        public float delta = 0.1f;

        public void OnButtonPressed()
        {
            ourScrollbar.value += delta;
        }
    }
}