using HexMap.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Map
{
   [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
   public class HexMesh : MonoBehaviour
   {
      #region Buffers
      static List<Vector3> Vertices = new List<Vector3>();
      static List<Color> Colors = new List<Color>();
      static List<int> Triangles = new List<int>();
      #endregion

      Mesh m_HexMesh = default;
      MeshRenderer m_MeshRenderer = default;
      MeshCollider m_Collider = default;

      void Awake()
      {
         GetComponent<MeshFilter>().mesh = m_HexMesh = new Mesh();
         m_MeshRenderer = GetComponent<MeshRenderer>();
         m_Collider = gameObject.AddComponent<MeshCollider>();

         m_HexMesh.name = "Hex Mesh";
      }

      #region Triangles 

      void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
      {
         int vertexIndex = Vertices.Count;
         Vertices.Add(Perturb(v1));
         Vertices.Add(Perturb(v2));
         Vertices.Add(Perturb(v3));
         Triangles.Add(vertexIndex);
         Triangles.Add(vertexIndex + 1);
         Triangles.Add(vertexIndex + 2);
      }

      void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3)
      {
         int vertexIndex = Vertices.Count;
         Vertices.Add(v1);
         Vertices.Add(v2);
         Vertices.Add(v3);
         Triangles.Add(vertexIndex);
         Triangles.Add(vertexIndex + 1);
         Triangles.Add(vertexIndex + 2);
      }

      public void Triangulate(HexCell[] cells)
      {
         m_HexMesh.Clear();
         Vertices.Clear();
         Triangles.Clear();
         Colors.Clear();

         for (int i = 0; i < cells.Length; i++)
         {
            Triangulate(cells[i]);
         }
         m_HexMesh.vertices = Vertices.ToArray();
         m_HexMesh.triangles = Triangles.ToArray();
         m_HexMesh.SetColors(Colors.ToArray());
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

         if (cell.HasRiverThroughEdge(direction))
         {
            eVertices.v3.y = cell.StreamBedY;
         }

         // Add Triangles
         TriangulateEdgeFan(center, eVertices, cell.Color);

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
            e1.v5 + bridge
         );

         if (cell.HasRiverThroughEdge(direction))
         {
            e2.v3.y = neighbor.StreamBedY;
         }

         if (cell.GetEdgeType(direction) == HexGrid.HexEdgeType.Slope)
         {
            TriangulateEdgeTerraces(e1, cell, e2, neighbor);
         }
         else
         {
            TriangulateEdgeStrip(e1, cell.Color, e2, neighbor.Color);
         }

         HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
         if (direction <= HexGrid.HexDirection.E && nextNeighbor != null)
         {
            Vector3 v5 = e1.v5 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.Position.y;

            if (cell.Elevation <= neighbor.Elevation)
            {
               if (cell.Elevation <= nextNeighbor.Elevation)
               {
                  TriangulateCorner(e1.v5, cell, e2.v5, neighbor, v5, nextNeighbor);
               }
               else
               {
                  TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
               }
            }
            else if (neighbor.Elevation <= nextNeighbor.Elevation)
            {
               TriangulateCorner(e2.v5, neighbor, v5, nextNeighbor, e1.v5, cell);
            }
            else
            {
               TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
            }
         }
      }

      void TriangulateEdgeTerraces(
         EdgeVertices begin, HexCell beginCell,
         EdgeVertices end, HexCell endCell)
      {
         EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
         Color c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, 1);

         TriangulateEdgeStrip(begin, beginCell.Color, e2, c2);


         for (int i = 2; i < HexMetrics.terraceSteps; i++)
         {
            EdgeVertices e1 = e2;
            Color c1 = c2;
            e2 = EdgeVertices.TerraceLerp(begin, end, i);
            c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i);
            TriangulateEdgeStrip(e1, c1, e2, c2);
         }

         TriangulateEdgeStrip(e2, c2, end, endCell.Color);
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
            AddTriangleColor(bottomCell.Color, leftCell.Color, rightCell.Color);
         }
      }

      void TriangulateCornerTerraces(
         Vector3 begin, HexCell beginCell,
         Vector3 left, HexCell leftCell,
         Vector3 right, HexCell rightCell)
      {
         Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
         Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
         Color c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);
         Color c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, 1);

         AddTriangle(begin, v3, v4);
         AddTriangleColor(beginCell.Color, c3, c4);

         for (int i = 2; i < HexMetrics.terraceSteps; i++)
         {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;
            v3 = HexMetrics.TerraceLerp(begin, left, i);
            v4 = HexMetrics.TerraceLerp(begin, right, i);
            c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
            c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, i);
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2, c3, c4);
         }

         AddQuad(v3, v4, left, right);
         AddQuadColor(c3, c4, leftCell.Color, rightCell.Color);
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
         Color boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);

         TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);

         if (leftCell.GetEdgeType(rightCell) == HexGrid.HexEdgeType.Slope)
         {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
         }
         else
         {
            AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
            AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
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
         Color boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, b);

         TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);

         if (leftCell.GetEdgeType(rightCell) == HexGrid.HexEdgeType.Slope)
         {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
         }
         else
         {
            AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
            AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
         }
      }

      void TriangulateBoundaryTriangle(
         Vector3 begin, HexCell beginCell,
         Vector3 left, HexCell leftCell,
         Vector3 boundary, Color boundaryColor)
      {
         Vector3 v2 = Perturb(HexMetrics.TerraceLerp(begin, left, 1));
         Color c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);

         AddTriangleUnperturbed(Perturb(begin), v2, boundary);
         AddTriangleColor(beginCell.Color, c2, boundaryColor);

         for (int i = 2; i < HexMetrics.terraceSteps; i++)
         {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = Perturb(HexMetrics.TerraceLerp(begin, left, i));
            c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
            AddTriangleUnperturbed(v1, v2, boundary);
            AddTriangleColor(c1, c2, boundaryColor);
         }

         AddTriangleUnperturbed(v2, Perturb(left), boundary);
         AddTriangleColor(c2, leftCell.Color, boundaryColor);
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
         AddTriangle(center, edge.v4, edge.v5);
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
         AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
         AddQuadColor(c1, c2);
      }

      void AddTriangleColor(Color color)
      {
         Colors.Add(color);
         Colors.Add(color);
         Colors.Add(color);
      }

      void AddTriangleColor(Color c1, Color c2, Color c3)
      {
         Colors.Add(c1);
         Colors.Add(c2);
         Colors.Add(c3);
      }

      #endregion 

      #region Quads

      void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
      {
         int vertexIndex = Vertices.Count;
         Vertices.Add(Perturb(v1));
         Vertices.Add(Perturb(v2));
         Vertices.Add(Perturb(v3));
         Vertices.Add(Perturb(v4));
         Triangles.Add(vertexIndex);
         Triangles.Add(vertexIndex + 2);
         Triangles.Add(vertexIndex + 1);
         Triangles.Add(vertexIndex + 1);
         Triangles.Add(vertexIndex + 2);
         Triangles.Add(vertexIndex + 3);
      }

      void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
      {
         Colors.Add(c1);
         Colors.Add(c2);
         Colors.Add(c3);
         Colors.Add(c4);
      }

      void AddQuadColor(Color c1, Color c2)
      {
         Colors.Add(c1);
         Colors.Add(c1);
         Colors.Add(c2);
         Colors.Add(c2);
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