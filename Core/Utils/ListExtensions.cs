using System;
using System.Collections.Generic;
using System.Linq;
using static Core.JobScheduling.Base.Machine;

namespace Core.Utils
{
    public static class ListExtensions
    {
        private static Random random = new Random();
        
        public static void Shuffle<T>(this IList<T> list)  
        {  
            int n = list.Count;  
            while (n > 1) 
            {  
                n--;  
                int k = random.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }

        public static List<T> Clone<T>(this IList<T> listToClone) where T: ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static void Replace<T>(this IList<T> list, T oldValue, IList<T> newValues)
        {
            var oldValueIndex = list.IndexOf(oldValue);

            // if (oldValueIndex == -1)
            //      throw new Exception("Element not found.");

            list[oldValueIndex] = newValues[0];

            for (int i = 1; i < newValues.Count; i++)
            {
                list.Insert(oldValueIndex + 1, newValues[i]);
            }
        }

        public static bool IsEqual<T>(this IList<T> list, IList<T> compareList) where T: TimeGap
        {
            if (list.Count != compareList.Count)
                return false;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].StartTime != compareList[i].StartTime || list[i].Duration != compareList[i].Duration)
                    return false;
            }

            return true;
        }

        public static void Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
    }
}