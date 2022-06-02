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
            [field: SerializeField] public int Id { get; }
            [field: SerializeField] public Material Material { get; }
            
            public Color Color => Material.color;

            public UniqueMaterial(int id, Material material)
            {
                Id = id;
                Material = material;
            }
        }

        [SerializeField] private List<UniqueMaterial> _uniqueMaterials;

        public UniqueMaterial[] UniqueMaterials => _uniqueMaterials.ToArray();

        public MaterialHolder()
        {
            _uniqueMaterials = new List<UniqueMaterial>();
        }
        
        public Material GetMaterial(int id)
        {
            return _uniqueMaterials.FirstOrDefault(m => m.Id == id)?.Material;
        }

        public void Register(UniqueMaterial uniqueMaterial)
        {
            if(_uniqueMaterials.Any(m => m.Id == uniqueMaterial.Id))
                return;
            
            _uniqueMaterials.Add(uniqueMaterial);
        }
    }
}