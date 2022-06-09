using System;
using UnityEngine;
using HexMap.Map;

namespace HexMap.Extensions
{
   #region Hex Map Direction

   public static class HexMapDirectionExtensions
   {
      /// <summary>
      /// Returns the opposite direction.
      /// </summary>
      public static HexGrid.HexDirection Opposite(this HexGrid.HexDirection direction)
      {
         return (int)direction < 3 ? (direction + 3) : (direction - 3);
      }

      /// <summary>
      /// Returns the previous direction.
      /// </summary>
      public static HexGrid.HexDirection Previous(this HexGrid.HexDirection direction)
      {
         return direction == HexGrid.HexDirection.NE ? HexGrid.HexDirection.NW : (direction - 1);
      }

      /// <summary>
      /// Returns the following direction.
      /// </summary>
      public static HexGrid.HexDirection Next(this HexGrid.HexDirection direction)
      {
         return direction == HexGrid.HexDirection.NW ? HexGrid.HexDirection.NE : (direction + 1);
      }

      public static HexGrid.HexDirection Previous2(this HexGrid.HexDirection direction)
      {
         direction -= 2;
         return direction >= HexGrid.HexDirection.NE ? direction : (direction + 6);
      }

      public static HexGrid.HexDirection Next2(this HexGrid.HexDirection direction)
      {
         direction += 2;
         return direction <= HexGrid.HexDirection.NW ? direction : (direction - 6);
      }
   }

   #endregion
}