using System.IO;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HexMap.Misc;
using HexMap.Units;
using HexMap.Map.Grid;
using System.ComponentModel;

namespace HexMap.Map {
   public class HexGrid : MonoBehaviour {
      [SerializeField] private HexGridSettingsSO _settings = default;

      private int chunkCountX,
         chunkCountZ,
         searchFrontierPhase;

      private bool currentPathExists;

      private HexCell currentPathFrom,
          currentPathTo;

      private HexCell[] cells = default;
      private HexGridChunk[] chunks = default;
      private HexCellPriorityQueue searchFrontier = default;
      private HexCellShaderData cellShaderData = default;
      private List<HexUnit> units = new List<HexUnit>();

      public bool HasPath {
         get {
            return currentPathExists;
         }
      }

      private void Awake() {
         HexMetrics.NoiseSource = _settings.NoiseSource;
         HexMetrics.InitializeHashGrid(_settings.Seed);
         HexUnit.unitPrefab = _settings.UnitPrefab;
         cellShaderData = gameObject.AddComponent<HexCellShaderData>();
         CreateMap(_settings.CellCountX, _settings.CellCountZ);
      }

      private void OnEnable() {
         if (!HexMetrics.NoiseSource) {
            HexMetrics.NoiseSource = _settings.NoiseSource;
            HexMetrics.InitializeHashGrid(_settings.Seed);
            HexUnit.unitPrefab = _settings.UnitPrefab;
            ResetVisibility();
         }
      }

      private void CreateChunks() {
         chunks = new HexGridChunk[chunkCountX * chunkCountZ];

         for (int z = 0, i = 0; z < chunkCountZ; z++) {
            for (int x = 0; x < chunkCountX; x++) {
               chunks[i++] = Instantiate(_settings.ChunkPrefab, transform);
            }
         }
      }

      private void CreateCells() {
         cells = new HexCell[_settings.CellCountZ * _settings.CellCountX];

         for (int z = 0, i = 0; z < _settings.CellCountZ; z++) {
            for (int x = 0; x < _settings.CellCountX; x++) {
               CreateCell(x, z, i++);
            }
         }
      }

      private void CreateCell(int x, int z, int i) {
         var position =
            new Vector3() {
               x = (x + z * 0.5f - z / 2) * (HexMetrics.InnerRadius * 2f),
               y = 0f,
               z = z * (HexMetrics.OuterRadius * 1.5f)
            };

         HexCell cell = cells[i] = Instantiate<HexCell>(_settings.CellPrefab);
         cell.transform.localPosition = position;
         cell.name = $"Cell #{i}";
         cell.Coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
         cell.Index = i;
         cell.ShaderData = cellShaderData;
         cell.Explorable = x >= 0 && z >= 0 && x < _settings.CellCountX && z < _settings.CellCountZ;

         if (x > 0) {
            cell.SetNeighbor(HexGridDirection.W, cells[i - 1]);
         }

         if (z > 0) {
            if ((z & 1) == 0) {
               cell.SetNeighbor(HexGridDirection.SE, cells[i - _settings.CellCountX]);
               if (x > 0) {
                  cell.SetNeighbor(HexGridDirection.SW, cells[i - _settings.CellCountX - 1]);
               }
            } else {
               cell.SetNeighbor(HexGridDirection.SW, cells[i - _settings.CellCountX]);
               if (x < _settings.CellCountX - 1) {
                  cell.SetNeighbor(HexGridDirection.SE, cells[i - _settings.CellCountX + 1]);
               }
            }
         }

         TextMeshProUGUI label = Instantiate<TextMeshProUGUI>(_settings.CellLabelPrefab);
         label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);

         cell.UIRect = label.rectTransform;
         cell.Elevation = 0;

         AddCellToChunk(x, z, cell);
      }

      private void AddCellToChunk(int x, int z, HexCell cell) {
         int chunkX = x / HexMetrics.ChunkSizeX;
         int chunkZ = z / HexMetrics.ChunkSizeZ;
         HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

         int localX = x - chunkX * HexMetrics.ChunkSizeX;
         int localZ = z - chunkZ * HexMetrics.ChunkSizeZ;
         int index = localX + localZ * HexMetrics.ChunkSizeX;
         chunk.AddCell(index, cell);
      }

      private void ClearUnits() {
         for (int i = 0; i < units.Count; i++) {
            units[i].Die();
         }
         units.Clear();
      }

      private void ShowPath(int speed) {
         if (currentPathExists) {
            HexCell current = currentPathTo;
            while (current != currentPathFrom) {
               int turn = current.Distance / speed;
               current.SetLabel(turn.ToString());
               current.EnableHighlight(Color.white);
               current = current.PathFrom;
            }
            currentPathFrom.EnableHighlight(Color.blue);
            currentPathTo.EnableHighlight(Color.red);
         }
      }

      private bool Search(HexCell fromCell, HexCell toCell, HexUnit unit) {
         searchFrontierPhase += 2;
         if (searchFrontier == null) {
            searchFrontier = new HexCellPriorityQueue();
         } else {
            searchFrontier.Clear();
         }

         fromCell.SearchPhase = searchFrontierPhase;
         fromCell.Distance = 0;
         searchFrontier.Enqueue(fromCell);
         while (searchFrontier.Count > 0) {
            HexCell current = searchFrontier.Dequeue();
            current.SearchPhase += 1;

            if (current == toCell) {
               return true;
            }

            int currentTurn = (current.Distance - 1) / unit.Speed;

            for (HexGridDirection dir = HexGridDirection.NE; dir <= HexGridDirection.NW; dir++) {
               HexCell neighbor = current.GetNeighbor(dir);
               if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase) {
                  continue;
               }

               if (!unit.IsValidDestination(neighbor)) {
                  continue;
               }
               int moveCost = unit.GetMoveCost(current, neighbor, dir);
               if (moveCost < 0) {
                  continue;
               }

               int distance = current.Distance + moveCost;
               int turn = (distance - 1) / unit.Speed;
               if (turn > currentTurn) {
                  distance = turn * unit.Speed + moveCost;
               }

               if (neighbor.SearchPhase < searchFrontierPhase) {
                  neighbor.SearchPhase = searchFrontierPhase;
                  neighbor.Distance = distance;
                  //neighbor.SetLabel(turn.ToString());
                  neighbor.PathFrom = current;
                  neighbor.SearchHeuristic = neighbor.Coordinates.DistanceTo(toCell.Coordinates);
                  searchFrontier.Enqueue(neighbor);
               } else if (distance < neighbor.Distance) {
                  int oldPriority = neighbor.SearchPriority;
                  neighbor.Distance = distance;
                  //neighbor.SetLabel(turn.ToString());
                  neighbor.PathFrom = current;
                  searchFrontier.Change(neighbor, oldPriority);
               }
            }
         }

         return false;
      }

      private List<HexCell> GetVisibleCells(HexCell fromCell, int range) {
         List<HexCell> visibleCells = ListPool<HexCell>.Get();

         searchFrontierPhase += 2;
         if (searchFrontier == null) {
            searchFrontier = new HexCellPriorityQueue();
         } else {
            searchFrontier.Clear();
         }

         range += fromCell.ViewElevation;
         fromCell.SearchPhase = searchFrontierPhase;
         fromCell.Distance = 0;
         searchFrontier.Enqueue(fromCell);
         HexCoordinates fromCoordinates = fromCell.Coordinates;
         while (searchFrontier.Count > 0) {
            HexCell current = searchFrontier.Dequeue();
            current.SearchPhase += 1;
            visibleCells.Add(current);

            for (HexGridDirection dir = HexGridDirection.NE; dir <= HexGridDirection.NW; dir++) {
               HexCell neighbor = current.GetNeighbor(dir);
               if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase || !neighbor.Explorable) {
                  continue;
               }

               int distance = current.Distance + 1;
               if (distance + neighbor.ViewElevation > range ||
                   distance > fromCoordinates.DistanceTo(neighbor.Coordinates)) {
                  continue;
               }

               if (neighbor.SearchPhase < searchFrontierPhase) {
                  neighbor.SearchPhase = searchFrontierPhase;
                  neighbor.Distance = distance;
                  neighbor.SearchHeuristic = 0;
                  searchFrontier.Enqueue(neighbor);
               } else if (distance < neighbor.Distance) {
                  int oldPriority = neighbor.SearchPriority;
                  neighbor.Distance = distance;
                  searchFrontier.Change(neighbor, oldPriority);
               }
            }
         }

         return visibleCells;
      }

      public HexCell GetCell(Vector3 position) {
         return GetCell(HexCoordinates.FromPosition(transform.InverseTransformPoint(position)));
      }

      public HexCell GetCell(HexCoordinates coordinates) {
         int z = coordinates.Z;
         if (z < 0 || z >= _settings.CellCountZ) {
            return null;
         }

         int x = coordinates.X + z / 2;
         if (x < 0 || x >= _settings.CellCountX) {
            return null;
         }

         return cells[x + z * _settings.CellCountX];
      }

      public HexCell GetCell(int xOffset, int zOffset) {
         return cells[xOffset + zOffset * _settings.CellCountX];
      }

      public HexCell GetCell(int cellIndex) {
         return cells[cellIndex];
      }

      public int GetCellCountX() {
         return _settings.CellCountX;
      }

      public int GetCellCountZ() {
         return _settings.CellCountZ;
      }

      public void ShowUI(bool visible) {
         for (int i = 0; i < chunks.Length; i++) {
            chunks[i].ShowUI(visible);
         }
      }

      public void Save(BinaryWriter writer) {
         writer.Write(_settings.CellCountX);
         writer.Write(_settings.CellCountZ);

         for (int i = 0; i < cells.Length; i++) {
            cells[i].Save(writer);
         }

         writer.Write(units.Count);
         for (int i = 0; i < units.Count; i++) {
            units[i].Save(writer);
         }
      }

      public void Load(BinaryReader reader, int header) {
         ClearPath();
         ClearUnits();
         int x = 20, z = 15;

         x = reader.ReadInt32();
         z = reader.ReadInt32();


         if (x != _settings.CellCountX || z != _settings.CellCountZ) {
            if (!CreateMap(x, z)) {
               return;
            }
         }

         bool originalImmediateMode = cellShaderData.ImmediateMode;
         cellShaderData.ImmediateMode = true;

         for (int i = 0; i < cells.Length; i++) {
            cells[i].Load(reader, header);
         }

         for (int i = 0; i < chunks.Length; i++) {
            chunks[i].Refresh();
         }

         int unitCount = reader.ReadInt32();
         for (int i = 0; i < unitCount; i++) {
            HexUnit.Load(reader, this);
         }

         cellShaderData.ImmediateMode = originalImmediateMode;
      }

      public void FindPath(HexCell fromCell, HexCell toCell, HexUnit unit) {
         ClearPath();
         currentPathFrom = fromCell;
         currentPathTo = toCell;
         currentPathExists = Search(fromCell, toCell, unit);
         ShowPath(unit.Speed);
      }

      public void IncreaseVisibility(HexCell fromCell, int range) {
         List<HexCell> cells = GetVisibleCells(fromCell, range);
         for (int i = 0; i < cells.Count; i++) {
            cells[i].IncreaseVisibility();
         }
         ListPool<HexCell>.Add(cells);
      }

      public void DecreaseVisibility(HexCell fromCell, int range) {
         List<HexCell> cells = GetVisibleCells(fromCell, range);
         for (int i = 0; i < cells.Count; i++) {
            cells[i].DecreaseVisibility();
         }
         ListPool<HexCell>.Add(cells);
      }

      public void AddUnit(HexUnit unit, HexCell location, float orientation) {
         units.Add(unit);
         unit.Grid = this;
         unit.transform.SetParent(transform, false);
         unit.Location = location;
         unit.Orientation = orientation;
      }

      public void RemoveUnit(HexUnit unit) {
         // Remove reference
         units.Remove(unit);
         // Destroy game object
         unit.Die();
      }

      public void ClearPath() {
         if (currentPathExists) {
            HexCell current = currentPathTo;
            while (current != currentPathFrom) {
               current.SetLabel(null);
               current.DisableHighlight();
               current = current.PathFrom;
            }
            current.DisableHighlight();
            currentPathExists = false;
         } else if (currentPathFrom) {
            currentPathFrom.DisableHighlight();
            currentPathTo.DisableHighlight();
         }
         currentPathFrom = currentPathTo = null;
      }

      public void ResetVisibility() {
         for (int i = 0; i < cells.Length; i++) {
            cells[i].ResetVisibility();
         }
         for (int i = 0; i < units.Count; i++) {
            HexUnit unit = units[i];
            IncreaseVisibility(unit.Location, unit.VisionRange);
         }
      }

      public bool CreateMap(int x, int z) {
         if (x <= 0 || x % HexMetrics.ChunkSizeX != 0 || z <= 0 || z % HexMetrics.ChunkSizeZ != 0) {
            Debug.LogError($"Unsupported map size, must be multiple of {HexMetrics.ChunkSizeX}.");
            return false;
         }

         ClearPath();
         ClearUnits();

         if (chunks != null) {
            for (int i = 0; i < chunks.Length; i++) {
               Destroy(chunks[i].gameObject);
            }
         }

         _settings.UpdateCellCountX(x);
         _settings.UpdatecellCoundZ(z);
         chunkCountX = _settings.CellCountX / HexMetrics.ChunkSizeX;
         chunkCountZ = _settings.CellCountZ / HexMetrics.ChunkSizeZ;
         cellShaderData.Initialize(_settings.CellCountX, _settings.CellCountZ);

         CreateChunks();
         CreateCells();

         return true;
      }

      public HexCell GetCell(Ray ray) {
         if (Physics.Raycast(ray, out RaycastHit hit)) {
            return GetCell(hit.point);
         }

         return null;
      }

      public List<HexCell> GetPath() {
         if (!currentPathExists) {
            return null;
         }

         List<HexCell> path = ListPool<HexCell>.Get();
         for (HexCell c = currentPathTo; c != currentPathFrom; c = c.PathFrom) {
            path.Add(c);
         }
         path.Add(currentPathFrom);
         path.Reverse();
         return path;
      }
   }
}