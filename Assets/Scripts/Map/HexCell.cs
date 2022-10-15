using HexMap.Extensions;
using HexMap.Units;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HexMap.Map
{
   public class HexCell : MonoBehaviour
   {
      int elevation = -1,
         waterLevel = -1,
         urbanLevel = 0,
         farmLevel = 0,
         plantLevel = 0,
         specialIndex = 0,
         terrainTypeIndex,
         distance;
      bool hasIncomingRiver,
         hasOutgoingRiver,
         walled;

      HexGrid.HexDirection incomingRiver, outgoingRiver;

      [SerializeField] bool[] _roads;
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
            RefreshPosition();
            ValidateRivers();

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
            ValidateRivers();
            Refresh();
         }
      }
      public int UrbanLevel
      {
         get
         {
            return urbanLevel;
         }
         set
         {
            if (urbanLevel != value)
            {
               urbanLevel = value;
               RefreshSelfOnly();
            }
         }
      }
      public int FarmLevel
      {
         get
         {
            return farmLevel;
         }
         set
         {
            if (farmLevel != value)
            {
               farmLevel = value;
               RefreshSelfOnly();
            }
         }
      }
      public int PlantLevel
      {
         get
         {
            return plantLevel;
         }
         set
         {
            if (plantLevel != value)
            {
               plantLevel = value;
               RefreshSelfOnly();
            }
         }
      }
      public int SpecialIndex
      {
         get
         {
            return specialIndex;
         }
         set
         {
            if (specialIndex != value && !HasRiver)
            {
               specialIndex = value;
               RemoveRoads();
               RefreshSelfOnly();
            }
         }
      }
      public int TerrainTypeIndex
      {
         get
         {
            return terrainTypeIndex;
         }
         set
         {
            if (terrainTypeIndex != value)
            {
               terrainTypeIndex = value;
               Refresh();
            }
         }
      }
      public int Distance
      {
         get
         {
            return distance;
         }
         set
         {
            distance = value;
         }
      }
      public int SearchPriority
      {
         get
         {
            return Distance + SearchHeuristic;
         }
      }
      public int SearchPhase { get; set; }

      public bool IsSpecial
      {
         get
         {
            return specialIndex > 0;
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
      public bool Walled
      {
         get
         {
            return walled;
         }
         set
         {
            if (walled != value)
            {
               walled = value;
               Refresh();
            }
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
      public HexCell NextWithSamePriority { get; set; }
      public HexUnit Unit { get; set; }
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

      [NonSerialized] public int SearchHeuristic = default;
      [NonSerialized] public HexGridChunk Chunk = default;
      [NonSerialized] public HexCoordinates Coordinates = default;
      [NonSerialized] public RectTransform UIRect = default;
      [NonSerialized] public HexCell PathFrom = default;

      void Refresh()
      {
         if (Chunk)
         {
            Chunk.Refresh();
            for (int i = 0; i < _neighbors.Length; i++)
            {
               HexCell neighbor = _neighbors[i];
               if (neighbor != null && neighbor.Chunk != Chunk)
               {
                  neighbor.Chunk.Refresh();
               }
            }
            if (Unit)
            {
               Unit.ValidateLocation();
            }
         }
      }

      void RefreshPosition()
      {
         Vector3 position = transform.localPosition;
         position.y = elevation * HexMetrics.elevationStep;
         position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
         transform.localPosition = position;

         Vector3 uiPosition = UIRect.localPosition;
         uiPosition.z = elevation * -HexMetrics.elevationStep;
         UIRect.localPosition = uiPosition;
      }

      void RefreshSelfOnly()
      {
         Chunk.Refresh();
         if (Unit)
         {
            Unit.ValidateLocation();
         }
      }

      public void EnableHighlight(Color color)
      {
         Image highlight = UIRect.GetChild(0).GetComponent<Image>();
         highlight.color = color;
         highlight.enabled = true;
      }

      public void DisableHighlight()
      {
         Image highlight = UIRect.GetChild(0).GetComponent<Image>();
         highlight.enabled = false;
      }

      public void SetLabel(string text)
      {
         TMP_Text label = UIRect.GetComponent<TMP_Text>();
         label.text = text;
      }

      #region Neighbors

      public void SetNeighbor(HexGrid.HexDirection direction, HexCell cell)
      {
         _neighbors[(int)direction] = cell;
         cell._neighbors[(int)direction.Opposite()] = this;
      }

      public HexCell GetNeighbor(HexGrid.HexDirection direction)
      {
         return _neighbors[(int)direction];
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

      #endregion

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
         if (!IsValidRiverDestination(neighbor))
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
         specialIndex = 0;

         neighbor.RemoveIncomingRiver();
         neighbor.hasIncomingRiver = true;
         neighbor.incomingRiver = direction.Opposite();
         neighbor.specialIndex = 0;

         SetRoad((int)direction, false);
      }

      bool IsValidRiverDestination(HexCell neighbor)
      {
         return neighbor && (
            elevation >= neighbor.elevation || waterLevel == neighbor.elevation
         );
      }

      void ValidateRivers()
      {
         if (hasOutgoingRiver && !IsValidRiverDestination(GetNeighbor(outgoingRiver)))
         {
            RemoveOutgoingRiver();
         }
         if (hasIncomingRiver && !GetNeighbor(incomingRiver).IsValidRiverDestination(this))
         {
            RemoveIncomingRiver();
         }
      }

      #endregion

      #region Roads

      public void AddRoad(HexGrid.HexDirection direction)
      {
         if (!_roads[(int)direction] && !HasRiverThroughEdge(direction) && !IsSpecial && !GetNeighbor(direction).IsSpecial && GetElevationDifference(direction) <= 1)
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

      #region Perist Data

      public void Save(BinaryWriter writer)
      {
         writer.Write((byte)terrainTypeIndex);
         writer.Write((byte)elevation);
         writer.Write((byte)waterLevel);
         writer.Write((byte)urbanLevel);
         writer.Write((byte)farmLevel);
         writer.Write((byte)plantLevel);
         writer.Write((byte)specialIndex);
         writer.Write(walled);

         if (hasIncomingRiver)
         {
            writer.Write((byte)(incomingRiver + 128));
         }
         else
         {
            writer.Write((byte)0);
         }

         if (hasOutgoingRiver)
         {
            writer.Write((byte)(outgoingRiver + 128));
         }
         else
         {
            writer.Write((byte)0);
         }

         int roadFlags = 0;
         for (int i = 0; i < _roads.Length; i++)
         {
            if (_roads[i])
            {
               roadFlags |= 1 << i;
            }
         }
         writer.Write((byte)roadFlags);
      }

      public void Load(BinaryReader reader)
      {
         terrainTypeIndex = reader.ReadByte();
         elevation = reader.ReadByte();
         RefreshPosition();
         waterLevel = reader.ReadByte();
         urbanLevel = reader.ReadByte();
         farmLevel = reader.ReadByte();
         plantLevel = reader.ReadByte();
         specialIndex = reader.ReadByte();
         walled = reader.ReadBoolean();

         byte riverData = reader.ReadByte();
         if (riverData >= 128)
         {
            hasIncomingRiver = true;
            incomingRiver = (HexGrid.HexDirection)(riverData - 128);
         }
         else
         {
            hasIncomingRiver = false;
         }

         riverData = reader.ReadByte();
         if (riverData >= 128)
         {
            hasOutgoingRiver = true;
            outgoingRiver = (HexGrid.HexDirection)(riverData - 128);
         }
         else
         {
            hasOutgoingRiver = false;
         }

         int roadFlags = reader.ReadByte();
         for (int i = 0; i < _roads.Length; i++)
         {
            _roads[i] = (roadFlags & (1 << i)) != 0;
         }
      }

      #endregion 
   }
}