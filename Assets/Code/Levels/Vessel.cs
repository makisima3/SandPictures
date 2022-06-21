using System.Collections.Generic;
using System.Linq;
using Code.InitDatas;
using DG.Tweening;
using Plugins.RobyyUtils;
using Plugins.SimpleFactory;
using UnityEngine;
using UnityEngine.Events;

namespace Code.Levels
{
    [RequireComponent(typeof(Grid))]
    public class Vessel : MonoBehaviour, IInitialized<VesselInitData>
    {
        [SerializeField] private float validCellPoint = 1f;
        [SerializeField] private float invalidCellPoint = 1f;
        [SerializeField] private Transform spawnPointView;
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] private ParticleSystem sandPS;

        private Grain[,] _grains;
        private float _step;
        
        private IOrderedEnumerable<IGrouping<Color, Cell>> _zones;

        public void Initialize(VesselInitData initData)
        {
            _grains = new Grain[initData.Size.x, initData.Size.y];
            
            var pos = spawnPointView.position;
            pos.x = pointA.position.x;
            spawnPointView.transform.position = pos;
            
            sandPS.gameObject.SetActive(false);

            initData.OnSpawnStateChange.AddListener((isSpawn) => { sandPS.gameObject.SetActive(isSpawn); });

            var distance = Vector3.Distance(pointA.position, pointB.position);
            _step = distance / initData.Size.x;
        }

        public void Move(Vector2Int position, float dropRate,Color color, bool onPS, UnityEvent onMoveEnd)
        {
            var pos = spawnPointView.position;
            pos.x = (pointB.position + Vector3.right * (_step * position.x)).x;
            
            spawnPointView
                .DOMove(pos, 
                    ExtraMathf.GetTime(Vector3.Distance(spawnPointView.position,pos),dropRate))
                .SetEase(Ease.Linear)
                .OnComplete(onMoveEnd.Invoke);
            sandPS.gameObject.SetActive(onPS);
            sandPS.startColor = color;
        }

        public float CompareResult(Cell[,] cells)
        {
            var correctGrainsCount = 0f;

            for (int x = 0; x < cells.GetLength(0); x++)
            {
                for (int y = 0; y < cells.GetLength(1); y++)
                {
                    if (cells[x, y].Color == _grains[x, y].Color)
                        correctGrainsCount++;
                }
            }

            return correctGrainsCount / cells.Length;
        }

        private Cell[,] _cells;

        public float CompareResultV2(Cell[,] cells)
        {
            _cells = cells;
            var correctGrainsCount = 0f;

            for (int x = 0; x < cells.GetLength(0); x++)
            {
                for (int y = 0; y < cells.GetLength(1); y++)
                {
                    if (cells[x, y].Color == _grains[x, y].Color)
                        correctGrainsCount += validCellPoint;
                    else
                        correctGrainsCount -= invalidCellPoint;
                }
            }

            return correctGrainsCount / (cells.Length * validCellPoint);
        }

        [ContextMenu("Compare")]
        private void Test()
        {
            Debug.Log(CompareResultV2(_cells));
        }

        public float Remap(float value, float fromLower, float fromUpper, float toLower, float toUpper)
        {
            return (toUpper - toLower) * ((value - fromLower) / (fromUpper - fromLower)) + toLower;
        }
    }
}