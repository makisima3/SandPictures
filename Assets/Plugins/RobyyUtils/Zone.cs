using UnityEngine;

namespace Plugins.RobyyUtils
{
    public class Zone : MonoBehaviour
    {
        [SerializeField]
        private Color editorColor = Color.green;
        [SerializeField]
        private float width, height;

        [SerializeField, HideInInspector]
        private Rect rectangle;

        public Rect Rectangle => rectangle;
        public Vector2 Size => new Vector2(width, height);

        public Vector2 RandomPosition() => new Vector2()
        {
            x = Random.Range(rectangle.xMin, rectangle.xMax),
            y = Random.Range(rectangle.yMin, rectangle.yMax)
        };

        private void OnDrawGizmos()
        {
            rectangle = new Rect
            {
                width = width,
                height = height,
                center = transform.position,
            };
            Gizmos.color = editorColor;
            GizmosExt.DrawRect(rectangle);
        }
    }
}
