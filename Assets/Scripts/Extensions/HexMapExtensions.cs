using System;
using UnityEngine;
using HexMap.Map;

namespace HexMap.Extensions
{
   #region Hex Map Direction
   public static class HexMapDirectionExtensions
   {
      public static HexGrid.HexDirection Opposite(this HexGrid.HexDirection direction)
      {
         return (int)direction < 3 ? (direction + 3) : (direction - 3);
      }

      public static HexGrid.HexDirection Previous(this HexGrid.HexDirection direction)
      {
         return direction == HexGrid.HexDirection.NE ? HexGrid.HexDirection.NW : (direction - 1);
      }

      public static HexGrid.HexDirection Next(this HexGrid.HexDirection direction)
      {
         return direction == HexGrid.HexDirection.NW ? HexGrid.HexDirection.NE : (direction + 1);
      }
   }
   #endregion 
}