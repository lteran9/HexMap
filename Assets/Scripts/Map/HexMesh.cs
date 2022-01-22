using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Map
{
   [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
   public class HexMesh : MonoBehaviour
   {
      Mesh m_HexMesh = default;
      MeshRenderer m_MeshRenderer = default;
      MeshCollider m_Collider = default;
      List<int> m_Triangles = default;
      List<Vector3> m_Vertices = default;
      List<Color> m_Colors = default;


      void Awake()
      {
         GetComponent<MeshFilter>().mesh = m_HexMesh = new Mesh();
         m_MeshRenderer = GetComponent<MeshRenderer>();
         m_Collider = gameObject.AddComponent<MeshCollider>();

         m_HexMesh.name = "Hex Mesh";
         m_Triangles = new List<int>();
         m_Vertices = new List<Vector3>();
         m_Colors = new List<Color>();
      }

      void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
      {
         int vertexIndex = m_Vertices.Count;
         m_Vertices.Add(v1);
         m_Vertices.Add(v2);
         m_Vertices.Add(v3);
         m_Triangles.Add(vertexIndex);
         m_Triangles.Add(vertexIndex + 1);
         m_Triangles.Add(vertexIndex + 2);
      }

      public void Triangulate(HexCell[] cells)
      {
         m_HexMesh.Clear();
         m_Vertices.Clear();
         m_Triangles.Clear();
         m_Colors.Clear();

         for (int i = 0; i < cells.Length; i++)
         {
            Triangulate(cells[i]);
         }
         m_HexMesh.vertices = m_Vertices.ToArray();
         m_HexMesh.triangles = m_Triangles.ToArray();
         m_HexMesh.SetColors(m_Colors.ToArray());
         m_HexMesh.RecalculateNormals();

         m_Collider.sharedMesh = m_HexMesh;
      }

      void Triangulate(HexCell cell)
      {
         Vector3 center = cell.transform.localPosition;
         for (int i = 0; i < 6; i++)
         {
            AddTriangle(
               center,
               center + HexMetrics.corners[i],
               center + HexMetrics.corners[i + 1]
            );
            AddTriangleColor(cell.color);
         }

      }

      void AddTriangleColor(Color color)
      {
         m_Colors.Add(color);
         m_Colors.Add(color);
         m_Colors.Add(color);
      }

   }
}