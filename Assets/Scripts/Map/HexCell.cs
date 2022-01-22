using HexMap.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Map
{
   public class HexCell : MonoBehaviour
   {
      [NonSerialized]
      public HexCoordinates coordinates = default;
      [NonSerialized]
      public Color color = default;
      [SerializeField]
      HexCell[] neighbors = default;

      public HexCell GetNeighbor(HexGrid.HexDirection direction)
      {
         return neighbors[(int)direction];
      }

      public void SetNeighbor(HexGrid.HexDirection direction, HexCell cell)
      {
         neighbors[(int)direction] = cell;
         cell.neighbors[(int)direction.Opposite()] = this;
      }
   }
}