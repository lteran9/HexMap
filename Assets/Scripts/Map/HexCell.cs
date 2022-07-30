using HexMap.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Map
{
   public class HexCell : MonoBehaviour
   {
      int elevation = -1, waterLevel = -1;
      bool hasIncomingRiver, hasOutgoingRiver;
      HexGrid.HexDirection incomingRiver, outgoingRiver;

      [SerializeField] bool[] _roads;
      [SerializeField] Color _color = default;
      [SerializeField] HexCell[] _neighbors = default;

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
            uiPosition.z = elevation * -HexMetrics.elevationStep;
            uiRect.localPosition = uiPosition;

            if (hasOutgoingRiver && elevation < GetNeighbor(outgoingRiver).elevation)
            {
               RemoveOutgoingRiver();
            }

            if (hasIncomingRiver && elevation > GetNeighbor(incomingRiver).elevation)
            {
               RemoveIncomingRiver();
            }

            for (int i = 0; i < _roads.Length; i++)
            {
               if (_roads[i] && GetElevationDifference((HexGrid.HexDirection)i) > 1)
               {
                  SetRoad(i, false);
               }
            }

            Refresh();
         }
      }
      public int WaterLevel
      {
         get
         {
            return waterLevel;
         }
         set
         {
            if (WaterLevel == value)
            {
               return;
            }

            waterLevel = value;
            Refresh();
         }
      }

      public bool HasRiver
      {
         get
         {
            return hasIncomingRiver || hasOutgoingRiver;
         }
      }
      public bool HasRiverBeginOrEnd
      {
         get
         {
            return hasIncomingRiver != hasOutgoingRiver;
         }
      }
      public bool HasIncomingRiver
      {
         get
         {
            return hasIncomingRiver;
         }
      }
      public bool HasOutgoingRiver
      {
         get
         {
            return hasOutgoingRiver;
         }
      }
      public bool HasRoads
      {
         get
         {
            for (int i = 0; i < _roads.Length; i++)
            {
               if (_roads[i])
               {
                  return true;
               }

            }

            return false;
         }
      }
      public bool IsUnderwater
      {
         get
         {
            return waterLevel > elevation;
         }
      }

      public float StreamBedY
      {
         get
         {
            return (elevation + HexMetrics.streamBedElevationOffset) * HexMetrics.elevationStep;
         }
      }
      public float RiverSurfaceY
      {
         get
         {
            return (elevation + HexMetrics.waterElevationOffset) * HexMetrics.elevationStep;
         }
      }
      public float WaterSurfaceY
      {
         get
         {
            return (waterLevel + HexMetrics.waterElevationOffset) * HexMetrics.elevationStep;
         }
      }

      public Vector3 Position
      {
         get
         {
            return transform.localPosition;
         }
      }
      public Color Color
      {
         get
         {
            return _color;
         }
         set
         {
            if (_color == value)
            {
               return;
            }
            _color = value;
            Refresh();
         }
      }
      public HexGrid.HexDirection IncomingRiver
      {
         get
         {
            return incomingRiver;
         }
      }
      public HexGrid.HexDirection OutgoingRiver
      {
         get
         {
            return outgoingRiver;
         }
      }
      public HexGrid.HexDirection RiverBeginOrEndDirection
      {
         get
         {
            return hasIncomingRiver ? incomingRiver : outgoingRiver;
         }
      }

      public HexGridChunk chunk = default;

      [NonSerialized] public HexCoordinates coordinates = default;
      [NonSerialized] public RectTransform uiRect = default;

      void Refresh()
      {
         if (chunk)
         {
            chunk.Refresh();
            for (int i = 0; i < _neighbors.Length; i++)
            {
               HexCell neighbor = _neighbors[i];
               if (neighbor != null && neighbor.chunk != chunk)
               {
                  neighbor.chunk.Refresh();
               }
            }
         }
      }

      void RefreshSelfOnly()
      {
         chunk.Refresh();
      }

      public HexCell GetNeighbor(HexGrid.HexDirection direction)
      {
         return _neighbors[(int)direction];
      }

      public void SetNeighbor(HexGrid.HexDirection direction, HexCell cell)
      {
         _neighbors[(int)direction] = cell;
         cell._neighbors[(int)direction.Opposite()] = this;
      }

      public HexGrid.HexEdgeType GetEdgeType(HexGrid.HexDirection direction)
      {
         return HexMetrics.GetEdgeType(
            elevation, _neighbors[(int)direction].elevation
         );
      }

      public HexGrid.HexEdgeType GetEdgeType(HexCell otherCell)
      {
         return HexMetrics.GetEdgeType(
            elevation, otherCell.elevation
         );
      }

      #region Rivers

      public void RemoveOutgoingRiver()
      {
         if (!hasOutgoingRiver)
         {
            return;
         }
         hasOutgoingRiver = false;
         RefreshSelfOnly();

         HexCell neighbor = GetNeighbor(outgoingRiver);
         neighbor.hasIncomingRiver = false;
         neighbor.RefreshSelfOnly();
      }

      public void RemoveIncomingRiver()
      {
         if (!hasIncomingRiver)
         {
            return;
         }
         hasIncomingRiver = false;
         RefreshSelfOnly();

         HexCell neighbor = GetNeighbor(incomingRiver);
         neighbor.hasOutgoingRiver = false;
         neighbor.RefreshSelfOnly();
      }

      public void RemoveRiver()
      {
         RemoveOutgoingRiver();
         RemoveIncomingRiver();
      }

      public bool HasRiverThroughEdge(HexGrid.HexDirection direction)
      {
         return
            hasIncomingRiver && incomingRiver == direction ||
            hasOutgoingRiver && outgoingRiver == direction;
      }

      public void SetOutgoingRiver(HexGrid.HexDirection direction)
      {
         if (hasOutgoingRiver && outgoingRiver == direction)
         {
            return;
         }

         HexCell neighbor = GetNeighbor(direction);
         if (!neighbor || elevation < neighbor.elevation)
         {
            return;
         }

         RemoveOutgoingRiver();
         if (hasIncomingRiver && incomingRiver == direction)
         {
            RemoveIncomingRiver();
         }

         hasOutgoingRiver = true;
         outgoingRiver = direction;

         neighbor.RemoveIncomingRiver();
         neighbor.hasIncomingRiver = true;
         neighbor.incomingRiver = direction.Opposite();

         SetRoad((int)direction, false);
      }

      #endregion

      #region Roads

      public void AddRoad(HexGrid.HexDirection direction)
      {
         if (!_roads[(int)direction] && !HasRiverThroughEdge(direction) && GetElevationDifference(direction) <= 1)
         {
            SetRoad((int)direction, true);
         }
      }

      public void SetRoad(int index, bool state)
      {
         _roads[index] = state;
         _neighbors[index]._roads[(int)((HexGrid.HexDirection)index).Opposite()] = state;
         _neighbors[index].RefreshSelfOnly();
         RefreshSelfOnly();
      }

      public void RemoveRoads()
      {
         for (int i = 0; i < _roads.Length; i++)
         {
            if (_roads[i])
            {
               SetRoad(i, false);
            }
         }
      }

      public bool HasRoadThroughEdge(HexGrid.HexDirection direction)
      {
         return _roads[(int)direction];
      }

      public int GetElevationDifference(HexGrid.HexDirection direction)
      {
         int difference = elevation - GetNeighbor(direction).elevation;
         return difference >= 0 ? difference : -difference;
      }

      #endregion

   }
}