using System;
using UnityEngine;
using HexMap.Map;
using HexMap.Map.Grid;

namespace HexMap.Extensions {
   public static class HexMapDirectionExtensions {
      /// <summary>
      /// Returns the opposite direction.
      /// </summary>
      public static HexDirection Opposite(this HexDirection direction) {
         return (int)direction < 3 ? (direction + 3) : (direction - 3);
      }

      /// <summary>
      /// Returns the previous direction.
      /// </summary>
      public static HexDirection Previous(this HexDirection direction) {
         return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
      }

      /// <summary>
      /// Returns the following direction.
      /// </summary>
      public static HexDirection Next(this HexDirection direction) {
         return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
      }
   }
}