using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexMap.Map;

namespace HexMap.Misc {
   public class ListPool<T> {
      private static Stack<List<T>> pool = new Stack<List<T>>();

      public static List<T> Get() {
         if (pool.Count > 0) {
            return pool.Pop();
         }

         return new List<T>();
      }

      public static void Add(List<T> list) {
         list.Clear();
         pool.Push(list);
      }
   }

   public class IntPool : ListPool<int> { }
   public class ColorPool : ListPool<Color> { }
   public class Vector2Pool : ListPool<Vector2> { }
   public class Vector3Pool : ListPool<Vector3> { }
   public class HexCellPool : ListPool<HexCell> { }
}