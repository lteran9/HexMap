using System;
using TMPro;
using UnityEngine;

namespace HexMap.Map
{
   public class HexGrid : MonoBehaviour
   {
      [NonSerialized]
      public Color defaultColor = Color.white;
      [NonSerialized]
      public Color touchedColor = Color.green;


      [SerializeField] int chunkCountX = 4;
      [SerializeField] int chunkCountZ = 3;
      [SerializeField] HexCell cellPrefab = default;
      [SerializeField] TextMeshProUGUI cellLabelPrefab = default;
      [SerializeField] Texture2D noiseSource = default;
      [SerializeField] HexGridChunk chunkPrefab = default;

      int cellCountX, cellCountZ;

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
         HexMetrics.noiseSource = noiseSource;

         cellCountX = chunkCountX * HexMetrics.chunkSizeX;
         cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;

         CreateChunks();
         CreateCells();
      }

      void OnEnable()
      {
         HexMetrics.noiseSource = noiseSource;
      }

      void CreateChunks()
      {
         m_Chunks = new HexGridChunk[chunkCountX * chunkCountZ];

         for (int z = 0, i = 0; z < chunkCountZ; z++)
         {
            for (int x = 0; x < chunkCountX; x++)
            {
               HexGridChunk chunk = m_Chunks[i++] = Instantiate(chunkPrefab);
               chunk.transform.SetParent(transform);
            }
         }
      }

      void CreateCells()
      {
         m_Cells = new HexCell[cellCountZ * cellCountX];

         for (int z = 0, i = 0; z < cellCountZ; z++)
         {
            for (int x = 0; x < cellCountX; x++)
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

         HexCell cell = m_Cells[i] = Instantiate<HexCell>(cellPrefab);
         cell.transform.localPosition = position;
         cell.name = $"Cell #{i}";
         cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
         cell.Color = defaultColor;

         if (x > 0)
         {
            cell.SetNeighbor(HexDirection.W, m_Cells[i - 1]);
         }

         if (z > 0)
         {
            if ((z & 1) == 0)
            {
               cell.SetNeighbor(HexDirection.SE, m_Cells[i - cellCountX]);
               if (x > 0)
               {
                  cell.SetNeighbor(HexDirection.SW, m_Cells[i - cellCountX - 1]);
               }
            }
            else
            {
               cell.SetNeighbor(HexDirection.SW, m_Cells[i - cellCountX]);
               if (x < cellCountX - 1)
               {
                  cell.SetNeighbor(HexDirection.SE, m_Cells[i - cellCountX + 1]);
               }
            }
         }

         TextMeshProUGUI label = Instantiate<TextMeshProUGUI>(cellLabelPrefab);
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
         int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
         return m_Cells[index];
      }

      public HexCell GetCell(HexCoordinates coordinates)
      {
         int z = coordinates.Z;
         if (z < 0 || z >= cellCountZ)
         {
            return null;
         }
         int x = coordinates.X + z / 2;
         if (x < 0 || x >= cellCountX)
         {
            return null;
         }
         return m_Cells[x + z * cellCountX];
      }

      public int GetChunkX()
      {
         return chunkCountX;
      }

      public int GetChunkZ()
      {
         return chunkCountZ;
      }

      public void ShowUI(bool visible)
      {
         for (int i = 0; i < m_Chunks.Length; i++)
         {
            m_Chunks[i].ShowUI(visible);
         }
      }
   }
}