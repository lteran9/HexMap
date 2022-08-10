using System;
using System.IO;
using TMPro;
using UnityEngine;

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
      [SerializeField] Color[] _colors;

      int chunkCountX, chunkCountZ;

      HexCell[] m_Cells = default;
      HexGridChunk[] m_Chunks = default;

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
         HexMetrics.colors = _colors;
         CreateMap(_cellCountX, _cellCountZ);
      }

      void OnEnable()
      {
         if (!HexMetrics.noiseSource)
         {
            HexMetrics.noiseSource = _noiseSource;
            HexMetrics.InitializeHashGrid(_seed);
            HexMetrics.colors = _colors;
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
         cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

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
         label.text = cell.coordinates.ToStringOnSeparateLines();

         cell.uiRect = label.rectTransform;
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
         for (int i = 0; i < m_Cells.Length; i++)
         {
            m_Cells[i].Save(writer);
         }
      }

      public void Load(BinaryReader reader)
      {
         for (int i = 0; i < m_Cells.Length; i++)
         {
            m_Cells[i].Load(reader);
         }

         for (int i = 0; i < m_Chunks.Length; i++)
         {
            m_Chunks[i].Refresh();
         }
      }

      public void CreateMap(int x, int z)
      {
         if (x <= 0 || x % HexMetrics.chunkSizeX != 0 || z <= 0 || z % HexMetrics.chunkSizeZ != 0)
         {
            Debug.LogError("Unsupported map size.");
            return;
         }
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
      }
   }
}