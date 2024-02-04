using System.IO;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HexMap.Misc;
using HexMap.Units;
using HexMap.Map.Grid;

namespace HexMap.Map {
   public class HexGrid : MonoBehaviour {
      [SerializeField] private int _seed = 0;
      [SerializeField] private int _cellCountX = 20; // Must be  multiple of 5
      [SerializeField] private int _cellCountZ = 15; // Must be multiple of 5
      [SerializeField] private Texture2D _noiseSource = default;
      [SerializeField] private HexGridChunk _chunkPrefab = default;
      [SerializeField] private HexCell _cellPrefab = default;
      [SerializeField] private TextMeshProUGUI _cellLabelPrefab = default;
      [SerializeField] private HexUnit _unitPrefab;

      private int chunkCountX,
         chunkCountZ,
         searchFrontierPhase;

      private bool currentPathExists;

      private HexCell currentPathFrom,
          currentPathTo;

      private HexCell[] m_Cells = default;
      private HexGridChunk[] m_Chunks = default;
      private HexCellPriorityQueue searchFrontier = default;
      private HexCellShaderData cellShaderData = default;

      private List<HexUnit> units = new List<HexUnit>();

      public bool HasPath {
         get {
            return currentPathExists;
         }
      }

      private void Awake() {
         HexMetrics.NoiseSource = _noiseSource;
         HexMetrics.InitializeHashGrid(_seed);
         HexUnit.unitPrefab = _unitPrefab;
         cellShaderData = gameObject.AddComponent<HexCellShaderData>();
         cellShaderData.Grid = this;
         CreateMap(_cellCountX, _cellCountZ);
      }

      private void OnEnable() {
         if (!HexMetrics.NoiseSource) {
            HexMetrics.NoiseSource = _noiseSource;
            HexMetrics.InitializeHashGrid(_seed);
            HexUnit.unitPrefab = _unitPrefab;
            ResetVisibility();
         }
      }

      private void CreateChunks() {
         m_Chunks = new HexGridChunk[chunkCountX * chunkCountZ];

         for (int z = 0, i = 0; z < chunkCountZ; z++) {
            for (int x = 0; x < chunkCountX; x++) {
               HexGridChunk chunk = m_Chunks[i++] = Instantiate(_chunkPrefab);
               chunk.transform.SetParent(transform);
            }
         }
      }

      private void CreateCells() {
         m_Cells = new HexCell[_cellCountZ * _cellCountX];

         for (int z = 0, i = 0; z < _cellCountZ; z++) {
            for (int x = 0; x < _cellCountX; x++) {
               CreateCell(x, z, i++);
            }
         }
      }

      private void CreateCell(int x, int z, int i) {
         Vector3 position;

         position.x = (x + z * 0.5f - z / 2) * (HexMetrics.InnerRadius * 2f);
         position.y = 0f;
         position.z = z * (HexMetrics.OuterRadius * 1.5f);

         HexCell cell = m_Cells[i] = Instantiate<HexCell>(_cellPrefab);
         cell.transform.localPosition = position;
         cell.name = $"Cell #{i}";
         cell.Coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
         cell.Index = i;
         cell.ShaderData = cellShaderData;

         cell.Explorable = x > 0 && z > 0 && x < _cellCountX - 1 && z < _cellCountZ - 1;

         if (x > 0) {
            cell.SetNeighbor(HexGridDirection.W, m_Cells[i - 1]);
         }

         if (z > 0) {
            if ((z & 1) == 0) {
               cell.SetNeighbor(HexGridDirection.SE, m_Cells[i - _cellCountX]);
               if (x > 0) {
                  cell.SetNeighbor(HexGridDirection.SW, m_Cells[i - _cellCountX - 1]);
               }
            } else {
               cell.SetNeighbor(HexGridDirection.SW, m_Cells[i - _cellCountX]);
               if (x < _cellCountX - 1) {
                  cell.SetNeighbor(HexGridDirection.SE, m_Cells[i - _cellCountX + 1]);
               }
            }
         }

         TextMeshProUGUI label = Instantiate<TextMeshProUGUI>(_cellLabelPrefab);
         label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
         //label.text = cell.coordinates.ToStringOnSeparateLines();

         cell.UIRect = label.rectTransform;
         cell.Elevation = 0;

         AddCellToChunk(x, z, cell);
      }

      private void AddCellToChunk(int x, int z, HexCell cell) {
         int chunkX = x / HexMetrics.ChunkSizeX;
         int chunkZ = z / HexMetrics.ChunkSizeZ;
         HexGridChunk chunk = m_Chunks[chunkX + chunkZ * chunkCountX];

         int localX = x - chunkX * HexMetrics.ChunkSizeX;
         int localZ = z - chunkZ * HexMetrics.ChunkSizeZ;
         chunk.AddCell(localX + localZ * HexMetrics.ChunkSizeX, cell);
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
         position = transform.InverseTransformPoint(position);
         HexCoordinates coordinates = HexCoordinates.FromPosition(position);
         int index = coordinates.X + coordinates.Z * _cellCountX + coordinates.Z / 2;
         return m_Cells[index];
      }

      public HexCell GetCell(HexCoordinates coordinates) {
         int z = coordinates.Z;
         if (z < 0 || z >= _cellCountZ) {
            return null;
         }
         int x = coordinates.X + z / 2;
         if (x < 0 || x >= _cellCountX) {
            return null;
         }
         return m_Cells[x + z * _cellCountX];
      }

      public HexCell GetCell(int xOffset, int zOffset) {
         return m_Cells[xOffset + zOffset * _cellCountX];
      }

      public HexCell GetCell(int cellIndex) {
         return m_Cells[cellIndex];
      }

      public int GetCellCountX() {
         return _cellCountX;
      }

      public int GetCellCountZ() {
         return _cellCountZ;
      }

      public void ShowUI(bool visible) {
         for (int i = 0; i < m_Chunks.Length; i++) {
            m_Chunks[i].ShowUI(visible);
         }
      }

      public void Save(BinaryWriter writer) {
         writer.Write(_cellCountX);
         writer.Write(_cellCountZ);

         for (int i = 0; i < m_Cells.Length; i++) {
            m_Cells[i].Save(writer);
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


         if (x != _cellCountX || z != _cellCountZ) {
            if (!CreateMap(x, z)) {
               return;
            }
         }

         bool originalImmediateMode = cellShaderData.ImmediateMode;
         cellShaderData.ImmediateMode = true;

         for (int i = 0; i < m_Cells.Length; i++) {
            m_Cells[i].Load(reader, header);
         }

         for (int i = 0; i < m_Chunks.Length; i++) {
            m_Chunks[i].Refresh();
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
         units.Remove(unit);
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
         for (int i = 0; i < m_Cells.Length; i++) {
            m_Cells[i].ResetVisibility();
         }
         for (int i = 0; i < units.Count; i++) {
            HexUnit unit = units[i];
            IncreaseVisibility(unit.Location, unit.VisionRange);
         }
      }

      public bool CreateMap(int x, int z) {
         if (x <= 0 || x % HexMetrics.ChunkSizeX != 0 || z <= 0 || z % HexMetrics.ChunkSizeZ != 0) {
            Debug.LogError("Unsupported map size.");
            return false;
         }

         ClearPath();
         ClearUnits();
         if (m_Chunks != null) {
            for (int i = 0; i < m_Chunks.Length; i++) {
               Destroy(m_Chunks[i].gameObject);
            }
         }

         _cellCountX = x;
         _cellCountZ = z;
         chunkCountX = _cellCountX / HexMetrics.ChunkSizeX;
         chunkCountZ = _cellCountZ / HexMetrics.ChunkSizeZ;
         cellShaderData.Initialize(_cellCountX, _cellCountZ);

         CreateChunks();
         CreateCells();

         return true;
      }

      public HexCell GetCell(Ray ray) {
         RaycastHit hit;
         if (Physics.Raycast(ray, out hit)) {
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