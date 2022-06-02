using System;
using System.Linq;
using UnityEngine;

namespace Plugins.SimpleFactory
{
    public class SimpleFactory : MonoBehaviour
    {
        [SerializeField] private GameObject[] registry;

        public TPrefab Create<TPrefab, TData>(TData initData, string nameQualifier = "")
            where TPrefab : IInitialized<TData>
        {
            var prefab = Find<TPrefab>(nameQualifier);
            var instance = Instantiate(prefab).GetComponent<TPrefab>();
            instance.Initialize(initData);

            return instance;
        }

        private GameObject Find<T>(string nameQualifier = "")
        {
            var typeFiltered = registry.Where(obj => obj.TryGetComponent<T>(out _));

            GameObject result;

            if (nameQualifier.Length > 0)
            {
                result = typeFiltered.FirstOrDefault(obj => obj.name == nameQualifier);
            }
            else
            {
                result = typeFiltered.FirstOrDefault();
            }

            if (result != null)
                return result;

            var nameError = "";
            if (nameQualifier.Length > 0)
            {
                nameError = $" and name {nameQualifier}";
            }
            throw new Exception($"Object with component {typeof(T)}{nameError} not found");
        }
    }
}