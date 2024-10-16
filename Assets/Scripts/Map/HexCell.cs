using HexMap.EditorTools;
using HexMap.Extensions;
using HexMap.Map.Grid;
using HexMap.Units;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HexMap.Map {
   public class HexCell : MonoBehaviour {
      [SerializeField, ReadOnly]
      private bool _hasIncomingRiver,
         _hasOutgoingRiver,
         _walled,
         _explored;
      [SerializeField] bool[] _roads;
      [SerializeField] HexCell[] _neighbors = default;

      private int elevation = -1,
         waterLevel = -1,
         urbanLevel = 0,
         farmLevel = 0,
         plantLevel = 0,
         specialIndex = 0,
         terrainTypeIndex,
         distance,
         visibility;

      private HexGridDirection incomingRiver,
         outgoingRiver;
      private HexGridChunk chunk = default;

      public int Index { get; set; }
      public int Elevation {
         get {
            return elevation;
         }
         set {
            if (elevation == value) {
               return;
            }

            int originalViewElevation = ViewElevation;
            elevation = value;
            if (ViewElevation != originalViewElevation) {
               ShaderData.ViewElevationChanged();
            }

            elevation = value;
            RefreshPosition();
            ValidateRivers();

            for (int i = 0; i < _roads.Length; i++) {
               if (_roads[i] && GetElevationDifference((HexGridDirection)i) > 1) {
                  SetRoad(i, false);
               }
            }

            Refresh();
         }
      }
      public int WaterLevel {
         get {
            return waterLevel;
         }
         set {
            if (WaterLevel == value) {
               return;
            }

            int originalViewElevation = ViewElevation;
            waterLevel = value;
            if (ViewElevation != originalViewElevation) {
               ShaderData.ViewElevationChanged();
            }

            ValidateRivers();
            Refresh();
         }
      }
      public int UrbanLevel {
         get {
            return urbanLevel;
         }
         set {
            if (urbanLevel != value) {
               urbanLevel = value;
               RefreshSelfOnly();
            }
         }
      }
      public int FarmLevel {
         get {
            return farmLevel;
         }
         set {
            if (farmLevel != value) {
               farmLevel = value;
               RefreshSelfOnly();
            }
         }
      }
      public int PlantLevel {
         get {
            return plantLevel;
         }
         set {
            if (plantLevel != value) {
               plantLevel = value;
               RefreshSelfOnly();
            }
         }
      }
      public int SpecialIndex {
         get {
            return specialIndex;
         }
         set {
            if (specialIndex != value && !HasRiver) {
               specialIndex = value;
               RemoveRoads();
               RefreshSelfOnly();
            }
         }
      }
      public int TerrainTypeIndex {
         get {
            return terrainTypeIndex;
         }
         set {
            if (terrainTypeIndex != value) {
               terrainTypeIndex = value;
               ShaderData.RefreshTerrain(this);
            }
         }
      }
      public int Distance {
         get {
            return distance;
         }
         set {
            distance = value;
         }
      }
      public int SearchPriority {
         get {
            return Distance + SearchHeuristic;
         }
      }
      public int ViewElevation {
         get {
            return elevation >= waterLevel ? elevation : waterLevel;
         }
      }
      public int SearchPhase { get; set; }

      public bool IsSpecial {
         get {
            return specialIndex > 0;
         }
      }
      public bool HasRiver {
         get {
            return _hasIncomingRiver || _hasOutgoingRiver;
         }
      }
      public bool HasRiverBeginOrEnd {
         get {
            return _hasIncomingRiver != _hasOutgoingRiver;
         }
      }
      public bool HasIncomingRiver {
         get {
            return _hasIncomingRiver;
         }
      }
      public bool HasOutgoingRiver {
         get {
            return _hasOutgoingRiver;
         }
      }
      public bool HasRoads {
         get {
            for (int i = 0; i < _roads.Length; i++) {
               if (_roads[i]) {
                  return true;
               }

            }

            return false;
         }
      }
      public bool IsUnderwater {
         get {
            return waterLevel > elevation;
         }
      }
      public bool Walled {
         get {
            return _walled;
         }
         set {
            if (_walled != value) {
               _walled = value;
               Refresh();
            }
         }
      }
      public bool IsVisible {
         get {
            return visibility > 0 && Explorable;
         }
      }
      public bool IsExplored {
         get {
            return _explored && Explorable;
         }
         set {
            _explored = value;
         }
      }
      public bool Explorable { get; set; }

      public float StreamBedY {
         get {
            return (elevation + HexMetrics.StreamBedElevationOffset) * HexMetrics.ElevationStep;
         }
      }
      public float RiverSurfaceY {
         get {
            return (elevation + HexMetrics.WaterElevationOffset) * HexMetrics.ElevationStep;
         }
      }
      public float WaterSurfaceY {
         get {
            return (waterLevel + HexMetrics.WaterElevationOffset) * HexMetrics.ElevationStep;
         }
      }

      public Vector3 Position {
         get {
            return transform.localPosition;
         }
      }
      public HexCell NextWithSamePriority { get; set; }
      public HexUnit Unit { get; set; }
      public HexGridDirection IncomingRiver {
         get {
            return incomingRiver;
         }
      }
      public HexGridDirection OutgoingRiver {
         get {
            return outgoingRiver;
         }
      }
      public HexGridDirection RiverBeginOrEndDirection {
         get {
            return _hasIncomingRiver ? incomingRiver : outgoingRiver;
         }
      }
      public HexCellShaderData ShaderData { get; set; }

      [NonSerialized] public int SearchHeuristic = default;
      [NonSerialized] public HexCoordinates Coordinates = default;
      [NonSerialized] public RectTransform UIRect = default;
      [NonSerialized] public HexCell PathFrom = default;

      private void Refresh() {
         if (chunk) {
            chunk.Refresh();
            for (int i = 0; i < _neighbors.Length; i++) {
               HexCell neighbor = _neighbors[i];
               if (neighbor != null && neighbor.chunk != chunk) {
                  neighbor.chunk.Refresh();
               }
            }
            if (Unit) {
               Unit.ValidateLocation();
            }
         }
      }

      private void RefreshPosition() {
         Vector3 position = transform.localPosition;
         position.y = elevation * HexMetrics.ElevationStep;
         position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.ElevationPerturbStrength;
         transform.localPosition = position;

         Vector3 uiPosition = UIRect.localPosition;
         uiPosition.z = elevation * -HexMetrics.ElevationStep;
         UIRect.localPosition = uiPosition;
      }

      private void RefreshSelfOnly() {
         chunk.Refresh();
         if (Unit) {
            Unit.ValidateLocation();
         }
      }

      public void SetChunk(HexGridChunk parent) {
         chunk = parent;
      }

      public void EnableHighlight(Color color) {
         Image highlight = UIRect.GetChild(0).GetComponent<Image>();
         highlight.color = color;
         highlight.enabled = true;
      }

      public void DisableHighlight() {
         Image highlight = UIRect.GetChild(0).GetComponent<Image>();
         highlight.enabled = false;
      }

      public void SetLabel(string text) {
         TMP_Text label = UIRect.GetComponent<TMP_Text>();
         label.text = text;
      }

      #region Visibility

      public void IncreaseVisibility() {
         visibility += 1;
         if (visibility >= 1) {
            IsExplored = true;
            ShaderData.RefreshVisibility(this);
         }
      }

      public void DecreaseVisibility() {
         visibility -= 1;
         if (visibility <= 0) {
            ShaderData.RefreshVisibility(this);
            visibility = 0;
         }
      }

      public void ResetVisibility() {
         if (visibility > 0) {
            visibility = 0;
            ShaderData.RefreshVisibility(this);
         }
      }

      #endregion

      #region Neighbors

      public void SetNeighbor(HexGridDirection direction, HexCell cell) {
         _neighbors[(int)direction] = cell;
         cell._neighbors[(int)direction.Opposite()] = this;
      }

      public HexCell GetNeighbor(HexGridDirection direction) {
         return _neighbors[(int)direction];
      }

      public HexEdgeType GetEdgeType(HexGridDirection direction) {
         return HexMetrics.GetEdgeType(
            elevation, _neighbors[(int)direction].elevation
         );
      }

      public HexEdgeType GetEdgeType(HexCell otherCell) {
         return HexMetrics.GetEdgeType(
            elevation, otherCell.elevation
         );
      }

      #endregion

      #region Rivers

      public void RemoveOutgoingRiver() {
         if (!_hasOutgoingRiver) {
            return;
         }
         _hasOutgoingRiver = false;
         RefreshSelfOnly();

         HexCell neighbor = GetNeighbor(outgoingRiver);
         neighbor._hasIncomingRiver = false;
         neighbor.RefreshSelfOnly();
      }

      public void RemoveIncomingRiver() {
         if (!_hasIncomingRiver) {
            return;
         }
         _hasIncomingRiver = false;
         RefreshSelfOnly();

         HexCell neighbor = GetNeighbor(incomingRiver);
         neighbor._hasOutgoingRiver = false;
         neighbor.RefreshSelfOnly();
      }

      public void RemoveRiver() {
         RemoveOutgoingRiver();
         RemoveIncomingRiver();
      }

      public bool HasRiverThroughEdge(HexGridDirection direction) {
         return
            _hasIncomingRiver && incomingRiver == direction ||
            _hasOutgoingRiver && outgoingRiver == direction;
      }

      public void SetOutgoingRiver(HexGridDirection direction) {
         if (_hasOutgoingRiver && outgoingRiver == direction) {
            return;
         }

         HexCell neighbor = GetNeighbor(direction);
         if (!IsValidRiverDestination(neighbor)) {
            return;
         }

         RemoveOutgoingRiver();
         if (_hasIncomingRiver && incomingRiver == direction) {
            RemoveIncomingRiver();
         }

         _hasOutgoingRiver = true;
         outgoingRiver = direction;
         specialIndex = 0;

         neighbor.RemoveIncomingRiver();
         neighbor._hasIncomingRiver = true;
         neighbor.incomingRiver = direction.Opposite();
         neighbor.specialIndex = 0;

         SetRoad((int)direction, false);
      }

      bool IsValidRiverDestination(HexCell neighbor) {
         return neighbor && (
            elevation >= neighbor.elevation || waterLevel == neighbor.elevation
         );
      }

      void ValidateRivers() {
         if (_hasOutgoingRiver && !IsValidRiverDestination(GetNeighbor(outgoingRiver))) {
            RemoveOutgoingRiver();
         }
         if (_hasIncomingRiver && !GetNeighbor(incomingRiver).IsValidRiverDestination(this)) {
            RemoveIncomingRiver();
         }
      }

      #endregion

      #region Roads

      public void AddRoad(HexGridDirection direction) {
         if (!_roads[(int)direction] && !HasRiverThroughEdge(direction) && !IsSpecial && !GetNeighbor(direction).IsSpecial && GetElevationDifference(direction) <= 1) {
            SetRoad((int)direction, true);
         }
      }

      public void SetRoad(int index, bool state) {
         _roads[index] = state;
         _neighbors[index]._roads[(int)((HexGridDirection)index).Opposite()] = state;
         _neighbors[index].RefreshSelfOnly();
         RefreshSelfOnly();
      }

      public void RemoveRoads() {
         for (int i = 0; i < _roads.Length; i++) {
            if (_roads[i]) {
               SetRoad(i, false);
            }
         }
      }

      public bool HasRoadThroughEdge(HexGridDirection direction) {
         return _roads[(int)direction];
      }

      public int GetElevationDifference(HexGridDirection direction) {
         int difference = elevation - GetNeighbor(direction).elevation;
         return difference >= 0 ? difference : -difference;
      }

      #endregion

      #region Perist Data

      public void Save(BinaryWriter writer) {
         writer.Write((byte)terrainTypeIndex);
         // Offset for negative numbers
         writer.Write((byte)(elevation + 127));
         writer.Write((byte)waterLevel);
         writer.Write((byte)urbanLevel);
         writer.Write((byte)farmLevel);
         writer.Write((byte)plantLevel);
         writer.Write((byte)specialIndex);
         writer.Write(_walled);

         if (_hasIncomingRiver) {
            writer.Write((byte)(incomingRiver + 128));
         } else {
            writer.Write((byte)0);
         }

         if (_hasOutgoingRiver) {
            writer.Write((byte)(outgoingRiver + 128));
         } else {
            writer.Write((byte)0);
         }

         int roadFlags = 0;
         for (int i = 0; i < _roads.Length; i++) {
            if (_roads[i]) {
               roadFlags |= 1 << i;
            }
         }
         writer.Write((byte)roadFlags);
         writer.Write(IsExplored);
      }

      public void Load(BinaryReader reader, int header) {
         terrainTypeIndex = reader.ReadByte();
         ShaderData.RefreshTerrain(this);
         elevation = reader.ReadByte();
         // Offset for potential negative number
         elevation -= 127;
         RefreshPosition();
         waterLevel = reader.ReadByte();
         urbanLevel = reader.ReadByte();
         farmLevel = reader.ReadByte();
         plantLevel = reader.ReadByte();
         specialIndex = reader.ReadByte();
         _walled = reader.ReadBoolean();

         byte riverData = reader.ReadByte();
         if (riverData >= 128) {
            _hasIncomingRiver = true;
            incomingRiver = (HexGridDirection)(riverData - 128);
         } else {
            _hasIncomingRiver = false;
         }

         riverData = reader.ReadByte();
         if (riverData >= 128) {
            _hasOutgoingRiver = true;
            outgoingRiver = (HexGridDirection)(riverData - 128);
         } else {
            _hasOutgoingRiver = false;
         }

         int roadFlags = reader.ReadByte();
         for (int i = 0; i < _roads.Length; i++) {
            _roads[i] = (roadFlags & (1 << i)) != 0;
         }

         IsExplored = reader.ReadBoolean();
         ShaderData.RefreshVisibility(this);
      }

      #endregion 
   }
}