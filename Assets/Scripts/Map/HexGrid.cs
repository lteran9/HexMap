using System;
using TMPro;
using UnityEngine;

namespace HexMap.Map
{
   public class HexGrid : MonoBehaviour
   {
      [NonSerialized]
      public int mapWidth = 6;
      [NonSerialized]
      public int mapHeight = 6;
      [NonSerialized]
      public Color defaultColor = Color.white;
      [NonSerialized]
      public Color touchedColor = Color.green;

      [SerializeField] HexCell cellPrefab = default;
      [SerializeField] TextMeshProUGUI cellLabelPrefab = default;

      Canvas m_GridCanvas = default;
      HexCell[] m_Cells = default;
      HexMesh m_HexMesh = default;

      public enum HexDirection
      {
         NE, E, SE, SW, W, NW
      }

      void Awake()
      {
         m_GridCanvas = GetComponentInChildren<Canvas>();
         m_HexMesh = GetComponentInChildren<HexMesh>();
         m_Cells = new HexCell[mapHeight * mapWidth];

         for (int z = 0, i = 0; z < mapHeight; z++)
         {
            for (int x = 0; x < mapWidth; x++)
            {
               CreateCell(x, z, i++);
            }
         }
      }

      void Start()
      {
         m_HexMesh.Triangulate(m_Cells);
      }

      void CreateCell(int x, int z, int i)
      {
         Vector3 position;
         position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
         position.y = 0f;
         position.z = z * (HexMetrics.outerRadius * 1.5f);

         HexCell cell = m_Cells[i] = Instantiate<HexCell>(cellPrefab);
         cell.transform.SetParent(m_HexMesh.transform, false);
         cell.transform.localPosition = position;
         cell.name = $"Cell #{i}";
         cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
         cell.color = defaultColor;

         if (x > 0)
         {
            cell.SetNeighbor(HexDirection.W, m_Cells[i - 1]);
         }

         if (z > 0)
         {
            if ((z & 1) == 0)
            {
               cell.SetNeighbor(HexDirection.SE, m_Cells[i - mapWidth]);
               if (x > 0)
               {
                  cell.SetNeighbor(HexDirection.SW, m_Cells[i - mapWidth - 1]);
               }
            }
            else
            {
               cell.SetNeighbor(HexDirection.SW, m_Cells[i - mapWidth]);
               if (x < mapWidth - 1)
               {
                  cell.SetNeighbor(HexDirection.SE, m_Cells[i - mapWidth + 1]);
               }
            }
         }

         TextMeshProUGUI label = Instantiate<TextMeshProUGUI>(cellLabelPrefab);
         label.rectTransform.SetParent(m_GridCanvas.transform, false);
         label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
         label.text = cell.coordinates.ToStringOnSeparateLines();
      }

      public void ColorCell(Vector3 position, Color color)
      {
         position = transform.InverseTransformPoint(position);
         HexCoordinates coordinates = HexCoordinates.FromPosition(position);
         int index = coordinates.X + coordinates.Z * mapWidth + coordinates.Z / 2;
         HexCell cell = m_Cells[index];
         cell.color = color;
         m_HexMesh.Triangulate(m_Cells);
      }
   }
}