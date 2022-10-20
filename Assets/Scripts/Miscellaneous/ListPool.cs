using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Misc
{
   public static class ListPool<T>
   {
      static Stack<List<T>> p_Stack = new Stack<List<T>>();

      public static List<T> Get()
      {
         if (p_Stack.Count > 0)
         {
            return p_Stack.Pop();
         }

         return new List<T>();
      }

      public static void Add(List<T> list)
      {
         list.Clear();
         p_Stack.Push(list);
      }
   }
}