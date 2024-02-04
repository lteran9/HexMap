using System;
using UnityEngine;
using HexMap.Map;
using HexMap.Map.Grid;

namespace HexMap.Extensions {
   public static class HexMapDirectionExtensions {
      /// <summary>
      /// Returns the opposite direction.
      /// </summary>
      public static HexGridDirection Opposite(this HexGridDirection direction) {
         return (int)direction < 3 ? (direction + 3) : (direction - 3);
      }

      /// <summary>
      /// Returns the previous direction.
      /// </summary>
      public static HexGridDirection Previous(this HexGridDirection direction) {
         return direction == HexGridDirection.NE ? HexGridDirection.NW : (direction - 1);
      }

      /// <summary>
      /// Returns the following direction.
      /// </summary>
      public static HexGridDirection Next(this HexGridDirection direction) {
         return direction == HexGridDirection.NW ? HexGridDirection.NE : (direction + 1);
      }
   }
}