using System.Collections.Generic;
using System.Linq;
using Code.InitDatas;
using DG.Tweening;
using OpenCover.Framework.Model;
using Plugins.RobyyUtils;
using Plugins.SimpleFactory;
using UnityEngine;

namespace Code.Levels
{
    public class ToolView : MonoBehaviour, IInitialized<ToolViewInitData>
    {
        public class ColoredToolPipe
        {
            public ToolPipeView ToolPipeView { get; set; }
            public Color Color { get; set; }
        }

        [SerializeField] private Transform view;
        [SerializeField] private List<ToolPipeView> toolPipeViews;
        [SerializeField] private float rimeToRotate = 0.1f;
        
        private float _rotationStep;
        private int _currentIndex;
        private Tween _rotateTween;
        
        public void Initialize(ToolViewInitData initData)
        {
            _rotationStep = 360f / toolPipeViews.Count();

            foreach (var toolPipeView in toolPipeViews)
            {
                toolPipeView.Initialize(new ToolPipeViewInitData());
            }
            
            toolPipeViews.First(p => p.Index == 0).SetColor(initData.firstColor);

            var otherPipes = toolPipeViews.Except(toolPipeViews.Where(p => p.Index == 0)).ToList();
            var clrs = initData.Colors.Except(initData.Colors.Where(c => c == initData.firstColor)).ChooseManyUnique(otherPipes.Count()).ToList();
            for (int i = 0; i < otherPipes.Count(); i++)
            {
               otherPipes[i].SetColor(clrs[i]);
            }
        }

        public void SetToolPipe(Color color)
        {
            _currentIndex += 2;

            if (_currentIndex >= toolPipeViews.Count() )
            {
                _currentIndex = Mathf.Abs(_currentIndex - toolPipeViews.Count());
            }

            toolPipeViews.First(p => p.Index == _currentIndex).SetColor(color);

            _rotateTween?.Kill();

            _rotateTween = view.DORotate(Vector3.up * (_rotationStep * _currentIndex) - Vector3.up , rimeToRotate,RotateMode.FastBeyond360).SetEase(Ease.Linear);
        }
    }
}