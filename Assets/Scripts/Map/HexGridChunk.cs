using HexMap.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HexMap.Map
{
   public class HexGridChunk : MonoBehaviour
   {
      Canvas gridCanvas = default;

      HexCell[] hexCells = default;

      [SerializeField] HexMesh _terrain = default;
      [SerializeField] HexMesh _rivers = default;
      [SerializeField] HexMesh _roads = default;
      [SerializeField] HexMesh _water = default;
      [SerializeField] HexMesh _waterShore = default;
      [SerializeField] HexMesh _estuaries = default;

      [SerializeField] HexFeatureManager _featureManager = default;

      void Awake()
      {
         gridCanvas = GetComponentInChildren<Canvas>();

         hexCells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];

         ShowUI(false);
      }

      void LateUpdate()
      {
         Triangulate(hexCells);
         enabled = false;
      }

      public void Refresh()
      {
         enabled = true;
      }

      public void ShowUI(bool visible)
      {
         gridCanvas.gameObject.SetActive(visible);
      }

      public void AddCell(int index, HexCell cell)
      {
         hexCells[index] = cell;
         cell.chunk = this;
         cell.transform.SetParent(transform, false);
         cell.uiRect.SetParent(gridCanvas.transform, false);
      }


      #region Triangulate 

      public void Triangulate(HexCell[] cells)
      {
         _terrain.Clear();
         _rivers.Clear();
         _roads.Clear();
         _water.Clear();
         _waterShore.Clear();
         _estuaries.Clear();
         _featureManager.Clear();

         for (int i = 0; i < cells.Length; i++)
         {
            Triangulate(cells[i]);
         }

         _terrain.Apply();
         _rivers.Apply();
         _roads.Apply();
         _water.Apply();
         _waterShore.Apply();
         _estuaries.Apply();
         _featureManager.Apply();
      }

      void Triangulate(HexCell cell)
      {
         for (var d = HexGrid.HexDirection.NE; d <= HexGrid.HexDirection.NW; d++)
         {
            Triangulate(d, cell);
         }

         if (!cell.IsUnderwater && !cell.HasRiver && !cell.HasRoads)
         {
            _featureManager.AddFeature(cell, cell.Position);
         }
      }

      void Triangulate(HexGrid.HexDirection direction, HexCell cell)
      {
         Vector3 center = cell.Position;
         EdgeVertices eVertices = new EdgeVertices(
            center + HexMetrics.GetFirstSolidCorner(direction),
            center + HexMetrics.GetSecondSolidCorner(direction)
         );

         if (cell.HasRiver)
         {
            if (cell.HasRiverThroughEdge(direction))
            {
               eVertices.v3.y = cell.StreamBedY;
               if (cell.HasRiverBeginOrEnd)
               {
                  TriangulateWithRiverBeginOrEnd(direction, cell, center, eVertices);
               }
               else
               {
                  TriangulateWithRiver(direction, cell, center, eVertices);
               }
            }
            else
            {
               TriangulateAdjacentToRiver(direction, cell, center, eVertices);
            }
         }
         else
         {
            TriangulateWithoutRiver(direction, cell, center, eVertices);

            if (!cell.IsUnderwater && !cell.HasRoadThroughEdge(direction))
            {
               _featureManager.AddFeature(cell, (center + eVertices.v1 + eVertices.v5) * (1f / 3f));
            }
         }

         if (direction <= HexGrid.HexDirection.SE)
         {
            TriangulateConnection(direction, cell, eVertices);
         }

         if (cell.IsUnderwater)
         {
            TriangulateWater(direction, cell, center);
         }
      }

      void TriangulateWithoutRiver(
         HexGrid.HexDirection direction,
         HexCell cell,
         Vector3 center,
         EdgeVertices eVertices)
      {
         TriangulateEdgeFan(center, eVertices, cell.Color);

         if (cell.HasRoads)
         {
            Vector2 interpolators = GetRoadInterpolators(direction, cell);
            TriangulateRoad(
               center,
               Vector3.Lerp(center, eVertices.v1, interpolators.x),
               Vector3.Lerp(center, eVertices.v5, interpolators.y),
               eVertices,
               cell.HasRoadThroughEdge(direction)
            );
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

         bool hasRiver = cell.HasRiverThroughEdge(direction);
         bool hasRoad = cell.HasRoadThroughEdge(direction);

         if (hasRiver)
         {
            e2.v3.y = neighbor.StreamBedY;

            if (!cell.IsUnderwater)
            {
               if (!neighbor.IsUnderwater)
               {
                  TriangulateRiverQuad(e1.v2, e1.v4, e2.v2, e2.v4,
                     cell.RiverSurfaceY, neighbor.RiverSurfaceY, 0.8f,
                     cell.HasIncomingRiver && cell.IncomingRiver == direction);
               }
               else if (cell.Elevation > neighbor.WaterLevel)
               {
                  TriangulateWaterfallInWater(
                     e1.v2, e1.v4, e2.v2, e2.v4,
                     cell.RiverSurfaceY, neighbor.RiverSurfaceY,
                     neighbor.WaterSurfaceY
                  );
               }
            }
            else if (!neighbor.IsUnderwater && neighbor.Elevation > cell.WaterLevel)
            {
               TriangulateWaterfallInWater(
                  e2.v4, e2.v2, e1.v4, e1.v2,
                  neighbor.RiverSurfaceY, cell.RiverSurfaceY,
                  cell.WaterSurfaceY
               );
            }
         }

         if (cell.GetEdgeType(direction) == HexGrid.HexEdgeType.Slope)
         {
            TriangulateEdgeTerraces(
               e1, cell, e2, neighbor, hasRoad
            );
         }
         else
         {
            TriangulateEdgeStrip(
               e1, cell.Color, e2, neighbor.Color, hasRoad
            );
         }

         _featureManager.AddWall(e1, cell, e2, neighbor, hasRiver, hasRoad);

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
            _terrain.AddTriangle(bottom, left, right);
            _terrain.AddTriangleColor(bottomCell.Color, leftCell.Color, rightCell.Color);
         }

         _featureManager.AddWall(bottom, bottomCell, left, leftCell, right, rightCell);
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

         _terrain.AddTriangle(begin, v3, v4);
         _terrain.AddTriangleColor(beginCell.Color, c3, c4);

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
            _terrain.AddQuad(v1, v2, v3, v4);
            _terrain.AddQuadColor(c1, c2, c3, c4);
         }

         _terrain.AddQuad(v3, v4, left, right);
         _terrain.AddQuadColor(c3, c4, leftCell.Color, rightCell.Color);
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

         Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(right), b);
         Color boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);

         TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);

         if (leftCell.GetEdgeType(rightCell) == HexGrid.HexEdgeType.Slope)
         {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
         }
         else
         {
            _terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
            _terrain.AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
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

         Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(left), b);
         Color boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, b);

         TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);

         if (leftCell.GetEdgeType(rightCell) == HexGrid.HexEdgeType.Slope)
         {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
         }
         else
         {
            _terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
            _terrain.AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
         }
      }

      void TriangulateBoundaryTriangle(
         Vector3 begin, HexCell beginCell,
         Vector3 left, HexCell leftCell,
         Vector3 boundary, Color boundaryColor)
      {
         Vector3 v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
         Color c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);

         _terrain.AddTriangleUnperturbed(HexMetrics.Perturb(begin), v2, boundary);
         _terrain.AddTriangleColor(beginCell.Color, c2, boundaryColor);

         for (int i = 2; i < HexMetrics.terraceSteps; i++)
         {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, i));
            c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
            _terrain.AddTriangleUnperturbed(v1, v2, boundary);
            _terrain.AddTriangleColor(c1, c2, boundaryColor);
         }

         _terrain.AddTriangleUnperturbed(v2, HexMetrics.Perturb(left), boundary);
         _terrain.AddTriangleColor(c2, leftCell.Color, boundaryColor);
      }

      void TriangulateEdgeFan(
         Vector3 center,
         EdgeVertices edge,
         Color color)
      {
         _terrain.AddTriangle(center, edge.v1, edge.v2);
         _terrain.AddTriangleColor(color);
         _terrain.AddTriangle(center, edge.v2, edge.v3);
         _terrain.AddTriangleColor(color);
         _terrain.AddTriangle(center, edge.v3, edge.v4);
         _terrain.AddTriangleColor(color);
         _terrain.AddTriangle(center, edge.v4, edge.v5);
         _terrain.AddTriangleColor(color);
      }

      void TriangulateEdgeStrip(
         EdgeVertices e1, Color c1,
         EdgeVertices e2, Color c2,
         bool hasRoad = false)
      {
         _terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
         _terrain.AddQuadColor(c1, c2);
         _terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
         _terrain.AddQuadColor(c1, c2);
         _terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
         _terrain.AddQuadColor(c1, c2);
         _terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
         _terrain.AddQuadColor(c1, c2);

         if (hasRoad)
         {
            TriangulateRoadSegment(e1.v2, e1.v3, e1.v4, e2.v2, e2.v3, e2.v4);
         }
      }

      void TriangulateEdgeTerraces(
         EdgeVertices begin, HexCell beginCell,
         EdgeVertices end, HexCell endCell,
         bool hasRoad)
      {
         EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
         Color c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, 1);

         TriangulateEdgeStrip(begin, beginCell.Color, e2, c2, hasRoad);


         for (int i = 2; i < HexMetrics.terraceSteps; i++)
         {
            EdgeVertices e1 = e2;
            Color c1 = c2;
            e2 = EdgeVertices.TerraceLerp(begin, end, i);
            c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i);
            TriangulateEdgeStrip(e1, c1, e2, c2, hasRoad);
         }

         TriangulateEdgeStrip(e2, c2, end, endCell.Color, hasRoad);
      }


      #region Rivers

      void TriangulateWithRiver(
         HexGrid.HexDirection direction,
         HexCell cell,
         Vector3 center,
         EdgeVertices eVertices)
      {
         Vector3 centerL;
         Vector3 centerR;

         if (cell.HasRiverThroughEdge(direction.Opposite()))
         {
            centerL = center + HexMetrics.GetFirstSolidCorner(direction.Previous()) * 0.25f;
            centerR = center + HexMetrics.GetSecondSolidCorner(direction.Next()) * 0.25f;
         }
         else if (cell.HasRiverThroughEdge(direction.Next()))
         {
            centerL = center;
            centerR = Vector3.Lerp(center, eVertices.v5, 2f / 3f);
         }
         else if (cell.HasRiverThroughEdge(direction.Previous()))
         {
            centerL = Vector3.Lerp(center, eVertices.v1, 2f / 3f);
            centerR = center;
         }
         else if (cell.HasRiverThroughEdge(direction.Next2()))
         {
            centerL = center;
            centerR = center + HexMetrics.GetSolidEdgeMiddle(direction.Next()) * (0.5f * HexMetrics.innerToOuter);
         }
         else
         {
            centerL = center + HexMetrics.GetSolidEdgeMiddle(direction.Previous()) * (0.5f * HexMetrics.innerToOuter);
            centerR = center;
         }

         center = Vector3.Lerp(centerL, centerR, 0.5f);

         EdgeVertices middle = new EdgeVertices(
            Vector3.Lerp(centerL, eVertices.v1, 0.5f),
            Vector3.Lerp(centerR, eVertices.v5, 0.5f),
            1f / 6f
         );

         middle.v3.y = center.y = eVertices.v3.y;

         TriangulateEdgeStrip(middle, cell.Color, eVertices, cell.Color);

         _terrain.AddTriangle(centerL, middle.v1, middle.v2);
         _terrain.AddTriangleColor(cell.Color);
         _terrain.AddQuad(centerL, center, middle.v2, middle.v3);
         _terrain.AddQuadColor(cell.Color);
         _terrain.AddQuad(center, centerR, middle.v3, middle.v4);
         _terrain.AddQuadColor(cell.Color);
         _terrain.AddTriangle(centerR, middle.v4, middle.v5);
         _terrain.AddTriangleColor(cell.Color);

         if (!cell.IsUnderwater)
         {
            bool reversed = cell.IncomingRiver == direction;
            TriangulateRiverQuad(centerL, centerR, middle.v2, middle.v4, cell.RiverSurfaceY, 0.4f, reversed);
            TriangulateRiverQuad(middle.v2, middle.v4, eVertices.v2, eVertices.v4, cell.RiverSurfaceY, 0.6f, reversed);
         }
      }

      void TriangulateWithRiverBeginOrEnd(
         HexGrid.HexDirection direction,
         HexCell cell,
         Vector3 center,
         EdgeVertices eVertices)
      {
         EdgeVertices middle = new EdgeVertices(
            Vector3.Lerp(center, eVertices.v1, 0.5f),
            Vector3.Lerp(center, eVertices.v5, 0.5f)
         );

         middle.v3.y = eVertices.v3.y;

         TriangulateEdgeStrip(middle, cell.Color, eVertices, cell.Color);
         TriangulateEdgeFan(center, middle, cell.Color);

         if (!cell.IsUnderwater)
         {
            bool reversed = cell.HasIncomingRiver;
            TriangulateRiverQuad(middle.v2, middle.v4, eVertices.v2, eVertices.v4, cell.RiverSurfaceY, 0.6f, reversed);

            center.y = middle.v2.y = middle.v4.y = cell.RiverSurfaceY;
            _rivers.AddTriangle(center, middle.v2, middle.v4);
            if (reversed)
            {
               _rivers.AddTriangleUV(
                  new Vector2(0.5f, 0.4f), new Vector2(1f, 0.2f), new Vector2(0f, 0.2f)
               );
            }
            else
            {
               _rivers.AddTriangleUV(
                  new Vector2(0.5f, 0.4f), new Vector2(0f, 0.6f), new Vector2(1f, 0.6f)
               );
            }
         }
      }

      void TriangulateAdjacentToRiver(
         HexGrid.HexDirection direction,
         HexCell cell,
         Vector3 center,
         EdgeVertices eVertices)
      {
         if (cell.HasRoads)
         {
            TriangulateRoadAdjacentToRiver(direction, cell, center, eVertices);
         }

         if (cell.HasRiverThroughEdge(direction.Next()))
         {
            if (cell.HasRiverThroughEdge(direction.Previous()))
            {
               center += HexMetrics.GetSolidEdgeMiddle(direction) * (HexMetrics.innerToOuter * 0.5f);
            }
            else if (cell.HasRiverThroughEdge(direction.Previous2()))
            {
               center += HexMetrics.GetFirstSolidCorner(direction) * 0.25f;
            }
         }
         else if (cell.HasRiverThroughEdge(direction.Previous()) && cell.HasRiverThroughEdge(direction.Next2()))
         {
            center += HexMetrics.GetSecondSolidCorner(direction) * 0.25f;
         }

         EdgeVertices mVertices = new EdgeVertices(
            Vector3.Lerp(center, eVertices.v1, 0.5f),
            Vector3.Lerp(center, eVertices.v5, 0.5f)
         );

         TriangulateEdgeStrip(mVertices, cell.Color, eVertices, cell.Color);
         TriangulateEdgeFan(center, mVertices, cell.Color);

         if (!cell.IsUnderwater && !cell.HasRoadThroughEdge(direction))
         {
            _featureManager.AddFeature(cell, (center + eVertices.v1 + eVertices.v5) * (1f / 3f));
         }
      }

      void TriangulateRiverQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y, float v, bool reversed)
      {
         TriangulateRiverQuad(v1, v2, v3, v4, y, y, v, reversed);
      }

      void TriangulateRiverQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y1, float y2, float v, bool reversed)
      {
         v1.y = v2.y = y1;
         v3.y = v4.y = y2;
         _rivers.AddQuad(v1, v2, v3, v4);
         if (reversed)
         {
            _rivers.AddQuadUV(1f, 0f, 0.8f - v, 0.6f - v);
         }
         else
         {
            _rivers.AddQuadUV(0f, 1f, v, v + 0.2f);
         }
      }

      #endregion

      #region Roads

      void TriangulateRoad(
         Vector3 center, Vector3 mL, Vector3 mR,
         EdgeVertices eVertices, bool hasRoadThroughCellEdge)
      {
         if (hasRoadThroughCellEdge)
         {
            Vector3 mC = Vector3.Lerp(mL, mR, 0.5f);
            TriangulateRoadSegment(mL, mC, mR, eVertices.v2, eVertices.v3, eVertices.v4);
            _roads.AddTriangle(center, mL, mC);
            _roads.AddTriangle(center, mC, mR);
            _roads.AddTriangleUV(
               new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f)
            );
            _roads.AddTriangleUV(
               new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f)
            );
         }
         else
         {
            TriangulateRoadEdge(center, mL, mR);
         }
      }

      void TriangulateRoadSegment(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6)
      {
         _roads.AddQuad(v1, v2, v4, v5);
         _roads.AddQuad(v2, v3, v5, v6);
         _roads.AddQuadUV(0f, 1f, 0f, 0f);
         _roads.AddQuadUV(1f, 0f, 0f, 0f);
      }

      void TriangulateRoadEdge(Vector3 center, Vector3 mL, Vector3 mR)
      {
         _roads.AddTriangle(center, mL, mR);
         _roads.AddTriangleUV(
            new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f)
         );
      }

      void TriangulateRoadAdjacentToRiver(
         HexGrid.HexDirection direction,
         HexCell cell,
         Vector3 center,
         EdgeVertices eVertices)
      {
         bool hasRoadThroughEdge = cell.HasRoadThroughEdge(direction);
         bool previousHasRiver = cell.HasRiverThroughEdge(direction.Previous());
         bool nextHasRiver = cell.HasRiverThroughEdge(direction.Next());
         Vector2 interpolators = GetRoadInterpolators(direction, cell);
         Vector3 roadCenter = center;

         if (cell.HasRiverBeginOrEnd)
         {
            roadCenter += HexMetrics.GetSolidEdgeMiddle(cell.RiverBeginOrEndDirection.Opposite()) * (1f / 3f);
         }
         else if (cell.IncomingRiver == cell.OutgoingRiver.Opposite())
         {
            Vector3 corner;
            if (previousHasRiver)
            {
               if (!hasRoadThroughEdge && !cell.HasRoadThroughEdge(direction.Next()))
               {
                  return;
               }
               corner = HexMetrics.GetSecondSolidCorner(direction);
            }
            else
            {
               if (!hasRoadThroughEdge && !cell.HasRoadThroughEdge(direction.Previous()))
               {
                  return;
               }
               corner = HexMetrics.GetFirstSolidCorner(direction);
            }
            roadCenter += corner * 0.5f;
            if (cell.IncomingRiver == direction.Next() &&
               (cell.HasRoadThroughEdge(direction.Next2()) || cell.HasRoadThroughEdge(direction.Opposite())))
            {
               _featureManager.AddBridge(roadCenter, center - corner * 0.5f);
            }
            center += corner * 0.25f;
         }
         else if (cell.IncomingRiver == cell.OutgoingRiver.Previous())
         {
            roadCenter -= HexMetrics.GetSecondCorner(cell.IncomingRiver) * 0.2f;
         }
         else if (cell.IncomingRiver == cell.OutgoingRiver.Next())
         {
            roadCenter -= HexMetrics.GetFirstCorner(cell.IncomingRiver) * 0.2f;
         }
         else if (previousHasRiver && nextHasRiver)
         {
            if (!hasRoadThroughEdge)
            {
               return;
            }
            Vector3 offset = HexMetrics.GetSolidEdgeMiddle(direction) * HexMetrics.innerToOuter;
            roadCenter += offset * 0.7f;
            center += offset * 0.5f;
         }
         else
         {
            HexGrid.HexDirection middle;
            if (previousHasRiver)
            {
               middle = direction.Next();
            }
            else if (nextHasRiver)
            {
               middle = direction.Previous();
            }
            else
            {
               middle = direction;
            }
            if (!cell.HasRoadThroughEdge(middle) && !cell.HasRoadThroughEdge(middle.Previous()) && !cell.HasRoadThroughEdge(middle.Next()))
            {
               return;
            }
            Vector3 offset = HexMetrics.GetSolidEdgeMiddle(middle);
            roadCenter += offset * 0.25f;
            if (direction == middle && cell.HasRoadThroughEdge(direction.Opposite()))
            {
               _featureManager.AddBridge(
                  roadCenter,
                  center - offset * (HexMetrics.innerToOuter * 0.7f)
               );
            }
         }

         Vector3 mL = Vector3.Lerp(roadCenter, eVertices.v1, interpolators.x);
         Vector3 mR = Vector3.Lerp(roadCenter, eVertices.v5, interpolators.y);
         TriangulateRoad(roadCenter, mL, mR, eVertices, hasRoadThroughEdge);
         if (previousHasRiver)
         {
            TriangulateRoadEdge(roadCenter, center, mL);
         }
         if (nextHasRiver)
         {
            TriangulateRoadEdge(roadCenter, mR, center);
         }
      }

      Vector2 GetRoadInterpolators(HexGrid.HexDirection direction, HexCell cell)
      {
         Vector2 interpolators;
         if (cell.HasRoadThroughEdge(direction))
         {
            interpolators.x = interpolators.y = 0.5f;
         }
         else
         {
            interpolators.x =
               cell.HasRoadThroughEdge(direction.Previous()) ? 0.5f : 0.25f;
            interpolators.y =
               cell.HasRoadThroughEdge(direction.Next()) ? 0.5f : 0.25f;
         }
         return interpolators;
      }

      #endregion

      #region Water

      void TriangulateWater(
            HexGrid.HexDirection direction, HexCell cell, Vector3 center
      )
      {
         center.y = cell.WaterSurfaceY;
         HexCell neighbor = cell.GetNeighbor(direction);
         if (neighbor != null && !neighbor.IsUnderwater)
         {
            TriangulateWaterShore(direction, cell, neighbor, center);
         }
         else
         {
            TriangulateOpenWater(direction, cell, neighbor, center);
         }
      }

      void TriangulateOpenWater(
         HexGrid.HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center
      )
      {
         Vector3 c1 = center + HexMetrics.GetFirstWaterCorner(direction);
         Vector3 c2 = center + HexMetrics.GetSecondWaterCorner(direction);

         _water.AddTriangle(center, c1, c2);

         if (direction <= HexGrid.HexDirection.SE && neighbor != null)
         {
            Vector3 bridge = HexMetrics.GetWaterBridge(direction);
            Vector3 e1 = c1 + bridge;
            Vector3 e2 = c2 + bridge;

            _water.AddQuad(c1, c2, e1, e2);

            if (direction <= HexGrid.HexDirection.E)
            {
               HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
               if (nextNeighbor == null || !nextNeighbor.IsUnderwater)
               {
                  return;
               }

               _water.AddTriangle(
                  c2, e2, c2 + HexMetrics.GetWaterBridge(direction.Next())
               );
            }
         }
      }

      void TriangulateWaterShore(
         HexGrid.HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center
      )
      {
         EdgeVertices e1 = new EdgeVertices(
            center + HexMetrics.GetFirstWaterCorner(direction),
            center + HexMetrics.GetSecondWaterCorner(direction)
         );
         _water.AddTriangle(center, e1.v1, e1.v2);
         _water.AddTriangle(center, e1.v2, e1.v3);
         _water.AddTriangle(center, e1.v3, e1.v4);
         _water.AddTriangle(center, e1.v4, e1.v5);

         Vector3 center2 = neighbor.Position;
         center2.y = center.y;
         EdgeVertices e2 = new EdgeVertices(
            center2 + HexMetrics.GetSecondSolidCorner(direction.Opposite()),
            center2 + HexMetrics.GetFirstSolidCorner(direction.Opposite())
         );

         if (cell.HasRiverThroughEdge(direction))
         {
            TriangulateEstuary(e1, e2, cell.IncomingRiver == direction);
         }
         else
         {
            _waterShore.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
            _waterShore.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
            _waterShore.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
            _waterShore.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
            _waterShore.AddQuadUV(0f, 0f, 0f, 1f);
            _waterShore.AddQuadUV(0f, 0f, 0f, 1f);
            _waterShore.AddQuadUV(0f, 0f, 0f, 1f);
            _waterShore.AddQuadUV(0f, 0f, 0f, 1f);
         }

         HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
         if (nextNeighbor != null)
         {
            Vector3 v3 = nextNeighbor.Position + (nextNeighbor.IsUnderwater ?
               HexMetrics.GetFirstWaterCorner(direction.Previous()) :
               HexMetrics.GetFirstSolidCorner(direction.Previous()));
            v3.y = center.y;
            _waterShore.AddTriangle(e1.v5, e2.v5, v3);
            _waterShore.AddTriangleUV(
               new Vector2(0f, 0f),
               new Vector2(0f, 1f),
               new Vector2(0f, nextNeighbor.IsUnderwater ? 0f : 1f)
            );
         }
      }

      #endregion

      #region Waterfalls

      void TriangulateWaterfallInWater(
         Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
         float y1, float y2, float waterY
      )
      {
         v1.y = v2.y = y1;
         v3.y = v4.y = y2;
         v1 = HexMetrics.Perturb(v1);
         v2 = HexMetrics.Perturb(v2);
         v3 = HexMetrics.Perturb(v3);
         v4 = HexMetrics.Perturb(v4);
         float t = (waterY - y2) / (y1 - y2);
         v3 = Vector3.Lerp(v3, v1, t);
         v4 = Vector3.Lerp(v4, v2, t);
         _rivers.AddQuadUnperturbed(v1, v2, v3, v4);
         _rivers.AddQuadUV(0f, 1f, 0.8f, 1f);
      }

      #endregion

      #region Estuaries

      void TriangulateEstuary(EdgeVertices e1, EdgeVertices e2, bool incomingRiver)
      {
         _waterShore.AddTriangle(e2.v1, e1.v2, e1.v1);
         _waterShore.AddTriangle(e2.v5, e1.v5, e1.v4);
         _waterShore.AddTriangleUV(
            new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f)
         );
         _waterShore.AddTriangleUV(
            new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f)
         );

         _estuaries.AddQuad(e2.v1, e1.v2, e2.v2, e1.v3);
         _estuaries.AddTriangle(e1.v3, e2.v2, e2.v4);
         _estuaries.AddQuad(e1.v3, e1.v4, e2.v4, e2.v5);

         _estuaries.AddQuadUV(
            new Vector2(0f, 1f), new Vector2(0f, 0f),
            new Vector2(0f, 1f), new Vector2(0f, 0f)
         );
         _estuaries.AddTriangleUV(
            new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 1f)
         );
         _estuaries.AddQuadUV(
            new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(1f, 1f), new Vector2(0f, 1f)
         );

         if (incomingRiver)
         {
            _estuaries.AddQuadUV2(
               new Vector2(1.5f, 1f), new Vector2(0.7f, 1.15f),
               new Vector2(1f, 0.8f), new Vector2(0.5f, 1.1f)
            );
            _estuaries.AddTriangleUV2(
               new Vector2(0.5f, 1.1f),
               new Vector2(1f, 0.8f),
               new Vector2(0f, 0.8f)
            );
            _estuaries.AddQuadUV2(
               new Vector2(0.5f, 1.1f), new Vector2(0.3f, 1.15f),
               new Vector2(0f, 0.8f), new Vector2(-0.5f, 1f)
            );
         }
         else
         {
            _estuaries.AddQuadUV2(
               new Vector2(-0.5f, -0.2f), new Vector2(0.3f, -0.35f),
               new Vector2(0f, 0f), new Vector2(0.5f, -0.3f)
            );
            _estuaries.AddTriangleUV2(
               new Vector2(0.5f, -0.3f),
               new Vector2(0f, 0f),
               new Vector2(1f, 0f)
            );
            _estuaries.AddQuadUV2(
               new Vector2(0.5f, -0.3f), new Vector2(0.7f, -0.35f),
               new Vector2(1f, 0f), new Vector2(1.5f, -0.2f)
            );
         }
      }

      #endregion 

      #endregion
   }
}