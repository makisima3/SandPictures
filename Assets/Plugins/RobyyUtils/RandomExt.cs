using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Plugins.RobyyUtils
{
    public static class RandomExt
    {
        public static bool Check(float chance) => Random.value <= chance;
        
        [CanBeNull]
        public static T ChooseOne<T>(this IEnumerable<T> values)
        {
            var valuesCast = values as T[] ?? values.ToArray();
            if (valuesCast.Length <= 0)
                return default;
            var i = Random.Range(0, valuesCast.Length);
            return valuesCast[i];
        }

        public static IEnumerable<T> ChooseMany<T>(this IEnumerable<T> values, int amount)
        {
            var result = new T[amount];
            var valuesCast = values as T[] ?? values.ToArray();
            
            for (int i = 0; i < amount; i++)
            {
                result[i] = valuesCast.ChooseOne();
            }

            return result;
        }

        public static IEnumerable<T> ChooseManyUnique<T>(this IEnumerable<T> values, int amount)
        {
            var valuesCast = values as T[] ?? values.ToArray();
            if (amount > valuesCast.Length)
            {
                amount = valuesCast.Length;
            }

            var result = new List<T>();
            
            while (amount > 0)
            {
                var candidate = valuesCast.ChooseOne();
                
                if(result.Contains(candidate))
                    continue;
                
                result.Add(candidate);
                amount -= 1;
            }

            return result;
        }
    }
}