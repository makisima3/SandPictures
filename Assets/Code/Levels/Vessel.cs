using System;
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
        [SerializeField] private float timeEdgeToPsOff = 0.5f;

        private Grain[,] _grains;
        private float _step;

        private IOrderedEnumerable<IGrouping<Color, Cell>> _zones;

        public void Initialize(VesselInitData initData)
        {
            _grains = new Grain[initData.Size.x, initData.Size.y];

            var pos = spawnPointView.position;
            pos.x = pointA.position.x;
            spawnPointView.transform.position = pos;

            sandPS.gameObject.SetActive(true);sandPS.startLifetime = 0;

            initData.OnSpawnStateChange.AddListener((isSpawn) => { sandPS.startLifetime = !isSpawn ? 0 : 15; });

            var distance = Vector3.Distance(pointA.position, pointB.position);
            _step = distance / initData.Size.x;
        }

        public Tween Move(Vector2Int position, float dropRate, Color color, bool onPS)
        {
            var pos = spawnPointView.position;
            pos.x = (pointB.position + Vector3.right * (_step * position.x)).x;
            var time = ExtraMathf.GetTime(Vector3.Distance(spawnPointView.position, pos), dropRate);

            if (time < timeEdgeToPsOff)
            {
                sandPS.startLifetime = 15;
               
            }
            else
            {
                time *= 2;
                sandPS.startLifetime = 0;
            }

            //sandPS.gameObject.SetActive(time < timeEdgeToPsOff);
            sandPS.startColor = color;
            
           return spawnPointView
                .DOMove(pos, time)
                .SetEase(Ease.Linear);
        }

        public int Position;
        public float _speed;
        private bool _isMoving;
        public void StartMove()
        {
            _isMoving = true;
        }

        public void StopMove()
        {
            _isMoving = false;
        }

        private void Update()
        {
            if(!_isMoving)
                return;
            
            var pos = spawnPointView.position;
            pos.x = (pointB.position + Vector3.right * (_step * Position)).x;
            spawnPointView.transform.position = Vector3.Lerp(spawnPointView.transform.position,pos,_speed);
        }
    }
}