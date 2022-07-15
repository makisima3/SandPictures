using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code
{
    [Serializable]
    public class MaterialHolder
    {
        [Serializable]
        public class UniqueMaterial
        {
            [field: SerializeField] public int Id { get; set; }
            [field: SerializeField] public Material Material { get; set; }
            
            public Color Color => Material.color;

            public UniqueMaterial(int id, Material material)
            {
                Id = id;
                Material = material;
            }
        }
        
        [Serializable]
        public class UniqueColor
        {
            [field: SerializeField] public int Id { get; set; }
            [field: SerializeField] public Color Color { get; set; }
            [field: SerializeField] public bool ShowInButtons { get; set; }

            public UniqueColor(int id, Color color, bool showInButtons)
            {
                Id = id;
                Color = color;
                ShowInButtons = showInButtons;
            }
        }
        

        [SerializeField] private List<UniqueMaterial> _uniqueMaterials;
        [SerializeField] public List<UniqueColor> _uniqueColors;
        
        public UniqueMaterial[] UniqueMaterials => _uniqueMaterials.ToArray();

        public MaterialHolder()
        {
            _uniqueMaterials = new List<UniqueMaterial>();
        }
        
        public Material GetMaterial(int id)
        {
            return _uniqueMaterials.FirstOrDefault(m => m.Id == id)?.Material;
        }
        
        public int? GetMaterialID(Color color)
        {
            return _uniqueMaterials.FirstOrDefault(m => m.Color == color)?.Id;
        }

        public void Register(UniqueMaterial uniqueMaterial)
        {
            if(_uniqueMaterials.Any(m => m.Id == uniqueMaterial.Id))
                return;
            
            _uniqueMaterials.Add(uniqueMaterial);
        }
    }
}