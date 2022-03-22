using HexMap.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Map
{
   public class HexCell : MonoBehaviour
   {
      int elevation = int.MinValue;

      public int Elevation
      {
         get
         {
            return elevation;
         }
         set
         {
            if (elevation == value)
            {
               return;
            }

            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = elevation * -position.y;
            uiRect.localPosition = uiPosition;
            Refresh();
         }
      }

      public Vector3 Position
      {
         get
         {
            return transform.localPosition;
         }
      }

      public HexGridChunk chunk = default;

      public Color Color
      {
         get
         {
            return color;
         }
         set
         {
            if (color == value)
            {
               return;
            }
            color = value;
            Refresh();
         }
      }

      [NonSerialized]
      public HexCoordinates coordinates = default;
      [NonSerialized]
      public RectTransform uiRect;

      Color color = default;
      [SerializeField]
      HexCell[] neighbors = default;

      void Refresh()
      {
         if (chunk)
         {
            chunk.Refresh();
            for (int i = 0; i < neighbors.Length; i++)
            {
               HexCell neighbor = neighbors[i];
               if (neighbor != null && neighbor.chunk != chunk)
               {
                  neighbor.chunk.Refresh();
               }
            }
         }
      }

      public HexCell GetNeighbor(HexGrid.HexDirection direction)
      {
         return neighbors[(int)direction];
      }

      public void SetNeighbor(HexGrid.HexDirection direction, HexCell cell)
      {
         neighbors[(int)direction] = cell;
         cell.neighbors[(int)direction.Opposite()] = this;
      }

      public HexGrid.HexEdgeType GetEdgeType(HexGrid.HexDirection direction)
      {
         return HexMetrics.GetEdgeType(
            elevation, neighbors[(int)direction].elevation
         );
      }

      public HexGrid.HexEdgeType GetEdgeType(HexCell otherCell)
      {
         return HexMetrics.GetEdgeType(
            elevation, otherCell.elevation
         );
      }
   }
}