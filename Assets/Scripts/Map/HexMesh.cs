using HexMap.Extensions;
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

      #region Triangles 

      void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
      {
         int vertexIndex = m_Vertices.Count;
         m_Vertices.Add(Perturb(v1));
         m_Vertices.Add(Perturb(v2));
         m_Vertices.Add(Perturb(v3));
         m_Triangles.Add(vertexIndex);
         m_Triangles.Add(vertexIndex + 1);
         m_Triangles.Add(vertexIndex + 2);
      }

      void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3)
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
         for (var d = HexGrid.HexDirection.NE; d <= HexGrid.HexDirection.NW; d++)
         {
            Triangulate(d, cell);
         }
      }

      void Triangulate(HexGrid.HexDirection direction, HexCell cell)
      {
         Vector3 center = cell.Position;
         EdgeVertices eVertices = new EdgeVertices(
            center + HexMetrics.GetFirstSolidCorner(direction),
            center + HexMetrics.GetSecondSolidCorner(direction)
         );

         // Add Triangles
         TriangulateEdgeFan(center, eVertices, cell.color);

         if (direction <= HexGrid.HexDirection.SE)
         {
            TriangulateConnection(direction, cell, eVertices);
         }
      }

      void TriangulateConnection(
         HexGrid.HexDirection direction,
         HexCell cell,
         EdgeVertices e1)
      {
         HexCell neighbor = cell.GetNeighbor(direction);
         if (neighbor == null)
         {
            return;
         }

         Vector3 bridge = HexMetrics.GetBridge(direction);
         bridge.y = neighbor.Position.y - cell.Position.y;
         EdgeVertices e2 = new EdgeVertices(
            e1.v1 + bridge,
            e1.v4 + bridge
         );

         if (cell.GetEdgeType(direction) == HexGrid.HexEdgeType.Slope)
         {
            TriangulateEdgeTerraces(e1, cell, e2, neighbor);
         }
         else
         {
            TriangulateEdgeStrip(e1, cell.color, e2, neighbor.color);
         }

         HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
         if (direction <= HexGrid.HexDirection.E && nextNeighbor != null)
         {
            Vector3 v5 = e1.v4 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.Position.y;

            if (cell.Elevation <= neighbor.Elevation)
            {
               if (cell.Elevation <= nextNeighbor.Elevation)
               {
                  TriangulateCorner(e1.v4, cell, e2.v4, neighbor, v5, nextNeighbor);
               }
               else
               {
                  TriangulateCorner(v5, nextNeighbor, e1.v4, cell, e2.v4, neighbor);
               }
            }
            else if (neighbor.Elevation <= nextNeighbor.Elevation)
            {
               TriangulateCorner(e2.v4, neighbor, v5, nextNeighbor, e1.v4, cell);
            }
            else
            {
               TriangulateCorner(v5, nextNeighbor, e1.v4, cell, e2.v4, neighbor);
            }
         }
      }

      void TriangulateEdgeTerraces(
         EdgeVertices begin, HexCell beginCell,
         EdgeVertices end, HexCell endCell)
      {
         EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
         Color c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, 1);

         TriangulateEdgeStrip(begin, beginCell.color, e2, c2);


         for (int i = 2; i < HexMetrics.terraceSteps; i++)
         {
            EdgeVertices e1 = e2;
            Color c1 = c2;
            e2 = EdgeVertices.TerraceLerp(begin, end, i);
            c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i);
            TriangulateEdgeStrip(e1, c1, e2, c2);
         }

         TriangulateEdgeStrip(e2, c2, end, endCell.color);
      }

      void TriangulateCorner(
         Vector3 bottom, HexCell bottomCell,
         Vector3 left, HexCell leftCell,
         Vector3 right, HexCell rightCell)
      {
         HexGrid.HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
         HexGrid.HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

         if (leftEdgeType == HexGrid.HexEdgeType.Slope)
         {
            if (rightEdgeType == HexGrid.HexEdgeType.Slope)
            {
               TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
            }
            else if (rightEdgeType == HexGrid.HexEdgeType.Flat)
            {
               TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
            }
            else
            {
               TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
            }
         }
         else if (rightEdgeType == HexGrid.HexEdgeType.Slope)
         {
            if (leftEdgeType == HexGrid.HexEdgeType.Flat)
            {
               TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
            }
            else
            {
               TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
            }
         }
         else if (leftCell.GetEdgeType(rightCell) == HexGrid.HexEdgeType.Slope)
         {
            if (leftCell.Elevation < rightCell.Elevation)
            {
               TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
            }
            else
            {
               TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
            }
         }
         else
         {
            AddTriangle(bottom, left, right);
            AddTriangleColor(bottomCell.color, leftCell.color, rightCell.color);
         }
      }

      void TriangulateCornerTerraces(
         Vector3 begin, HexCell beginCell,
         Vector3 left, HexCell leftCell,
         Vector3 right, HexCell rightCell)
      {
         Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
         Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
         Color c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);
         Color c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, 1);

         AddTriangle(begin, v3, v4);
         AddTriangleColor(beginCell.color, c3, c4);

         for (int i = 2; i < HexMetrics.terraceSteps; i++)
         {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;
            v3 = HexMetrics.TerraceLerp(begin, left, i);
            v4 = HexMetrics.TerraceLerp(begin, right, i);
            c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
            c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, i);
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2, c3, c4);
         }

         AddQuad(v3, v4, left, right);
         AddQuadColor(c3, c4, leftCell.color, rightCell.color);
      }

      void TriangulateCornerTerracesCliff(
         Vector3 begin, HexCell beginCell,
         Vector3 left, HexCell leftCell,
         Vector3 right, HexCell rightCell)
      {
         float b = 1f / (rightCell.Elevation - beginCell.Elevation);
         if (b < 0)
         {
            b = -b;
         }

         Vector3 boundary = Vector3.Lerp(Perturb(begin), Perturb(right), b);
         Color boundaryColor = Color.Lerp(beginCell.color, rightCell.color, b);

         TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);

         if (leftCell.GetEdgeType(rightCell) == HexGrid.HexEdgeType.Slope)
         {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
         }
         else
         {
            AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
            AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
         }
      }

      void TriangulateCornerCliffTerraces(
         Vector3 begin, HexCell beginCell,
         Vector3 left, HexCell leftCell,
         Vector3 right, HexCell rightCell)
      {
         float b = 1f / (leftCell.Elevation - beginCell.Elevation);
         if (b < 0)
         {
            b = -b;
         }

         Vector3 boundary = Vector3.Lerp(Perturb(begin), Perturb(left), b);
         Color boundaryColor = Color.Lerp(beginCell.color, leftCell.color, b);

         TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);

         if (leftCell.GetEdgeType(rightCell) == HexGrid.HexEdgeType.Slope)
         {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
         }
         else
         {
            AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
            AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
         }
      }

      void TriangulateBoundaryTriangle(
         Vector3 begin, HexCell beginCell,
         Vector3 left, HexCell leftCell,
         Vector3 boundary, Color boundaryColor)
      {
         Vector3 v2 = Perturb(HexMetrics.TerraceLerp(begin, left, 1));
         Color c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);

         AddTriangleUnperturbed(Perturb(begin), v2, boundary);
         AddTriangleColor(beginCell.color, c2, boundaryColor);

         for (int i = 2; i < HexMetrics.terraceSteps; i++)
         {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = Perturb(HexMetrics.TerraceLerp(begin, left, i));
            c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
            AddTriangleUnperturbed(v1, v2, boundary);
            AddTriangleColor(c1, c2, boundaryColor);
         }

         AddTriangleUnperturbed(v2, Perturb(left), boundary);
         AddTriangleColor(c2, leftCell.color, boundaryColor);
      }

      void TriangulateEdgeFan(
         Vector3 center,
         EdgeVertices edge,
         Color color)
      {
         AddTriangle(center, edge.v1, edge.v2);
         AddTriangleColor(color);
         AddTriangle(center, edge.v2, edge.v3);
         AddTriangleColor(color);
         AddTriangle(center, edge.v3, edge.v4);
         AddTriangleColor(color);
      }

      void TriangulateEdgeStrip(
         EdgeVertices e1, Color c1,
         EdgeVertices e2, Color c2)
      {
         AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
         AddQuadColor(c1, c2);
         AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
         AddQuadColor(c1, c2);
         AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
         AddQuadColor(c1, c2);
      }

      void AddTriangleColor(Color color)
      {
         m_Colors.Add(color);
         m_Colors.Add(color);
         m_Colors.Add(color);
      }

      void AddTriangleColor(Color c1, Color c2, Color c3)
      {
         m_Colors.Add(c1);
         m_Colors.Add(c2);
         m_Colors.Add(c3);
      }

      #endregion 

      #region Quads

      void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
      {
         int vertexIndex = m_Vertices.Count;
         m_Vertices.Add(Perturb(v1));
         m_Vertices.Add(Perturb(v2));
         m_Vertices.Add(Perturb(v3));
         m_Vertices.Add(Perturb(v4));
         m_Triangles.Add(vertexIndex);
         m_Triangles.Add(vertexIndex + 2);
         m_Triangles.Add(vertexIndex + 1);
         m_Triangles.Add(vertexIndex + 1);
         m_Triangles.Add(vertexIndex + 2);
         m_Triangles.Add(vertexIndex + 3);
      }

      void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
      {
         m_Colors.Add(c1);
         m_Colors.Add(c2);
         m_Colors.Add(c3);
         m_Colors.Add(c4);
      }

      void AddQuadColor(Color c1, Color c2)
      {
         m_Colors.Add(c1);
         m_Colors.Add(c1);
         m_Colors.Add(c2);
         m_Colors.Add(c2);
      }

      #endregion

      Vector3 Perturb(Vector3 position)
      {
         Vector4 sample = HexMetrics.SampleNoise(position);
         position.x += ((sample.x * 2f - 1) * HexMetrics.cellPerturbStrength);
         //position.y += ((sample.y * 2f - 1) * HexMetrics.cellPerturbStrength);
         position.z += ((sample.z * 2f - 1) * HexMetrics.cellPerturbStrength);
         return position;
      }
   }
}