using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HexMap.Map
{
   public class HexGridChunk : MonoBehaviour
   {
      HexMesh hexMesh = default;
      Canvas gridCanvas = default;

      HexCell[] m_Cells = default;

      void Awake()
      {
         gridCanvas = GetComponentInChildren<Canvas>();
         hexMesh = GetComponentInChildren<HexMesh>();

         m_Cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
      }

      // Start is called before the first frame update
      void Start()
      {
         hexMesh.Triangulate(m_Cells);
      }

      // Update is called once per frame
      void Update()
      {

      }

      public void AddCell(int index, HexCell cell)
      {
         m_Cells[index] = cell;
         cell.transform.SetParent(transform, false);
         cell.uiRect.SetParent(gridCanvas.transform, false);
      }

      public void Refresh()
      {
         hexMesh.Triangulate(m_Cells);
      }
   }
}