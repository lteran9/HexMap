using HexMap.Units;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HexMap.Map
{
   public class HexGrid : MonoBehaviour
   {
      [SerializeField] int _seed = 0;
      [SerializeField] int _cellCountX = 20;
      [SerializeField] int _cellCountZ = 15;
      [SerializeField] Texture2D _noiseSource = default;
      [SerializeField] HexGridChunk _chunkPrefab = default;
      [SerializeField] HexCell _cellPrefab = default;
      [SerializeField] TextMeshProUGUI _cellLabelPrefab = default;

      int chunkCountX,
         chunkCountZ,
         searchFrontierPhase;

      bool currentPathExists;

      HexCell currentPathFrom,
         currentPathTo;

      HexCell[] m_Cells = default;
      HexGridChunk[] m_Chunks = default;
      HexCellPriorityQueue searchFrontier = default;

      List<HexUnit> units = new List<HexUnit>();

      public bool HasPath
      {
         get
         {
            return currentPathExists;
         }
      }

      public HexUnit unitPrefab;

      public enum HexDirection
      {
         NE, E, SE, SW, W, NW
      }

      public enum HexEdgeType
      {
         Flat, Slope, Cliff
      }

      void Awake()
      {
         HexMetrics.noiseSource = _noiseSource;
         HexMetrics.InitializeHashGrid(_seed);
         HexUnit.unitPrefab = unitPrefab;
         CreateMap(_cellCountX, _cellCountZ);
      }

      void OnEnable()
      {
         if (!HexMetrics.noiseSource)
         {
            HexMetrics.noiseSource = _noiseSource;
            HexMetrics.InitializeHashGrid(_seed);
            HexUnit.unitPrefab = unitPrefab;
         }
      }

      void CreateChunks()
      {
         m_Chunks = new HexGridChunk[chunkCountX * chunkCountZ];

         for (int z = 0, i = 0; z < chunkCountZ; z++)
         {
            for (int x = 0; x < chunkCountX; x++)
            {
               HexGridChunk chunk = m_Chunks[i++] = Instantiate(_chunkPrefab);
               chunk.transform.SetParent(transform);
            }
         }
      }

      void CreateCells()
      {
         m_Cells = new HexCell[_cellCountZ * _cellCountX];

         for (int z = 0, i = 0; z < _cellCountZ; z++)
         {
            for (int x = 0; x < _cellCountX; x++)
            {
               CreateCell(x, z, i++);
            }
         }
      }

      void CreateCell(int x, int z, int i)
      {
         Vector3 position;

         position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
         position.y = 0f;
         position.z = z * (HexMetrics.outerRadius * 1.5f);

         HexCell cell = m_Cells[i] = Instantiate<HexCell>(_cellPrefab);
         cell.transform.localPosition = position;
         cell.name = $"Cell #{i}";
         cell.Coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

         if (x > 0)
         {
            cell.SetNeighbor(HexDirection.W, m_Cells[i - 1]);
         }

         if (z > 0)
         {
            if ((z & 1) == 0)
            {
               cell.SetNeighbor(HexDirection.SE, m_Cells[i - _cellCountX]);
               if (x > 0)
               {
                  cell.SetNeighbor(HexDirection.SW, m_Cells[i - _cellCountX - 1]);
               }
            }
            else
            {
               cell.SetNeighbor(HexDirection.SW, m_Cells[i - _cellCountX]);
               if (x < _cellCountX - 1)
               {
                  cell.SetNeighbor(HexDirection.SE, m_Cells[i - _cellCountX + 1]);
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

      void AddCellToChunk(int x, int z, HexCell cell)
      {
         int chunkX = x / HexMetrics.chunkSizeX;
         int chunkZ = z / HexMetrics.chunkSizeZ;
         HexGridChunk chunk = m_Chunks[chunkX + chunkZ * chunkCountX];

         int localX = x - chunkX * HexMetrics.chunkSizeX;
         int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
         chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
      }

      void ClearUnits()
      {
         for (int i = 0; i < units.Count; i++)
         {
            units[i].Die();
         }
         units.Clear();
      }

      void ShowPath(int speed)
      {
         if (currentPathExists)
         {
            HexCell current = currentPathTo;
            while (current != currentPathFrom)
            {
               int turn = current.Distance / speed;
               current.SetLabel(turn.ToString());
               current.EnableHighlight(Color.white);
               current = current.PathFrom;
            }
            currentPathFrom.EnableHighlight(Color.blue);
            currentPathTo.EnableHighlight(Color.red);
         }
      }

      bool Search(HexCell fromCell, HexCell toCell, int speed)
      {
         searchFrontierPhase += 2;
         if (searchFrontier == null)
         {
            searchFrontier = new HexCellPriorityQueue();
         }
         else
         {
            searchFrontier.Clear();
         }

         fromCell.SearchPhase = searchFrontierPhase;
         fromCell.Distance = 0;
         searchFrontier.Enqueue(fromCell);
         while (searchFrontier.Count > 0)
         {
            HexCell current = searchFrontier.Dequeue();
            current.SearchPhase += 1;

            if (current == toCell)
            {
               return true;
            }

            int currentTurn = current.Distance / speed;

            for (HexDirection dir = HexDirection.NE; dir <= HexDirection.NW; dir++)
            {
               HexCell neighbor = current.GetNeighbor(dir);
               if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase)
               {
                  continue;
               }
               if (neighbor.IsUnderwater || neighbor.Unit)
               {
                  continue;
               }
               HexEdgeType edgeType = current.GetEdgeType(neighbor);
               if (edgeType == HexEdgeType.Cliff)
               {
                  continue;
               }
               int moveCost;
               if (current.HasRoadThroughEdge(dir))
               {
                  moveCost = 1;
               }
               else if (current.Walled != neighbor.Walled)
               {
                  continue;
               }
               else
               {
                  moveCost = edgeType == HexEdgeType.Flat ? 5 : 10;
                  moveCost += neighbor.UrbanLevel + neighbor.FarmLevel + neighbor.PlantLevel;
               }

               int distance = current.Distance + moveCost;
               int turn = distance / speed;
               if (turn > currentTurn)
               {
                  distance = turn * speed + moveCost;
               }

               if (neighbor.SearchPhase < searchFrontierPhase)
               {
                  neighbor.SearchPhase = searchFrontierPhase;
                  neighbor.Distance = distance;
                  //neighbor.SetLabel(turn.ToString());
                  neighbor.PathFrom = current;
                  neighbor.SearchHeuristic = neighbor.Coordinates.DistanceTo(toCell.Coordinates);
                  searchFrontier.Enqueue(neighbor);
               }
               else if (distance < neighbor.Distance)
               {
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

      public HexCell GetCell(Vector3 position)
      {
         position = transform.InverseTransformPoint(position);
         HexCoordinates coordinates = HexCoordinates.FromPosition(position);
         int index = coordinates.X + coordinates.Z * _cellCountX + coordinates.Z / 2;
         return m_Cells[index];
      }

      public HexCell GetCell(HexCoordinates coordinates)
      {
         int z = coordinates.Z;
         if (z < 0 || z >= _cellCountZ)
         {
            return null;
         }
         int x = coordinates.X + z / 2;
         if (x < 0 || x >= _cellCountX)
         {
            return null;
         }
         return m_Cells[x + z * _cellCountX];
      }

      public int GetCellCountX()
      {
         return _cellCountX;
      }

      public int GetCellCountZ()
      {
         return _cellCountZ;
      }

      public void ShowUI(bool visible)
      {
         for (int i = 0; i < m_Chunks.Length; i++)
         {
            m_Chunks[i].ShowUI(visible);
         }
      }

      public void Save(BinaryWriter writer)
      {
         writer.Write(_cellCountX);
         writer.Write(_cellCountZ);

         for (int i = 0; i < m_Cells.Length; i++)
         {
            m_Cells[i].Save(writer);
         }

         writer.Write(units.Count);
         for (int i = 0; i < units.Count; i++)
         {
            units[i].Save(writer);
         }
      }

      public void Load(BinaryReader reader, int header)
      {
         ClearPath();
         ClearUnits();
         int x = 20, z = 15;
         if (header >= 1)
         {
            x = reader.ReadInt32();
            z = reader.ReadInt32();
         }

         if (x != _cellCountX || z != _cellCountZ)
         {
            if (!CreateMap(x, z))
            {
               return;
            }
         }

         for (int i = 0; i < m_Cells.Length; i++)
         {
            m_Cells[i].Load(reader);
         }

         for (int i = 0; i < m_Chunks.Length; i++)
         {
            m_Chunks[i].Refresh();
         }

         if (header >= 2)
         {
            int unitCount = reader.ReadInt32();
            for (int i = 0; i < unitCount; i++)
            {
               HexUnit.Load(reader, this);
            }
         }
      }

      public void FindPath(HexCell fromCell, HexCell toCell, int speed)
      {
         ClearPath();
         currentPathFrom = fromCell;
         currentPathTo = toCell;
         currentPathExists = Search(fromCell, toCell, speed);
         ShowPath(speed);
      }

      public void AddUnit(HexUnit unit, HexCell location, float orientation)
      {
         units.Add(unit);
         unit.transform.SetParent(transform, false);
         unit.Location = location;
         unit.Orientation = orientation;
      }

      public void RemoveUnit(HexUnit unit)
      {
         units.Remove(unit);
         unit.Die();
      }

      public void ClearPath()
      {
         if (currentPathExists)
         {
            HexCell current = currentPathTo;
            while (current != currentPathFrom)
            {
               current.SetLabel(null);
               current.DisableHighlight();
               current = current.PathFrom;
            }
            current.DisableHighlight();
            currentPathExists = false;
         }
         else if (currentPathFrom)
         {
            currentPathFrom.DisableHighlight();
            currentPathTo.DisableHighlight();
         }
         currentPathFrom = currentPathTo = null;
      }

      public bool CreateMap(int x, int z)
      {
         if (x <= 0 || x % HexMetrics.chunkSizeX != 0 || z <= 0 || z % HexMetrics.chunkSizeZ != 0)
         {
            Debug.LogError("Unsupported map size.");
            return false;
         }
         ClearPath();
         ClearUnits();
         if (m_Chunks != null)
         {
            for (int i = 0; i < m_Chunks.Length; i++)
            {
               Destroy(m_Chunks[i].gameObject);
            }
         }

         _cellCountX = x;
         _cellCountZ = z;
         chunkCountX = _cellCountX / HexMetrics.chunkSizeX;
         chunkCountZ = _cellCountZ / HexMetrics.chunkSizeZ;

         CreateChunks();
         CreateCells();

         return true;
      }

      public HexCell GetCell(Ray ray)
      {
         RaycastHit hit;
         if (Physics.Raycast(ray, out hit))
         {
            return GetCell(hit.point);
         }
         return null;
      }
   }
}