using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexMap.Map;

namespace HexMap.Misc {
   public class ListPool<T> {
      private static Stack<List<T>> pool = new Stack<List<T>>();

      public static List<T> Get() {
         // If pool has lists available pop one from stack
         if (pool.Count > 0) {
            return pool.Pop();
         }
         // If no pools available create one
         return new List<T>();
      }

      public static void Add(List<T> list) {
         // Clear values when adding back to pool
         list.Clear();
         // Push list to pool
         pool.Push(list);
      }
   }

   public class IntPool : ListPool<int> { }
   public class ColorPool : ListPool<Color> { }
   public class Vector2Pool : ListPool<Vector2> { }
   public class Vector3Pool : ListPool<Vector3> { }
   public class HexCellPool : ListPool<HexCell> { }
}