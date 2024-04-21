using HexMap.Misc;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using HexMap.EditorTools;
using HexMap.Map.Grid;

namespace HexMap.Map {
   [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
   public class HexMesh : MonoBehaviour {

      [SerializeField, ReadOnly]
      private bool useCollider,
               useCellData,
               useUVCoordinates,
               useUV2Coordinates;

      #region Buffers

      private List<int> triangles = default;

      private List<Color> cellWeights = default;

      private List<Vector2> uvs = default,
         uvs2 = default;

      private List<Vector3> vertices = default,
         cellIndices = default;

      #endregion

      private Mesh hexMesh = default;
      private MeshCollider meshCollider = default;

      private void Awake() {
         GetComponent<MeshFilter>().mesh = hexMesh = new Mesh() { name = $"{name} Hex Mesh" };
         if (useCollider) {
            meshCollider = gameObject.AddComponent<MeshCollider>();
         }
      }

      public void Clear() {
         hexMesh.Clear();
         vertices = Vector3Pool.Get();

         if (useCellData) {
            cellWeights = ColorPool.Get();
            cellIndices = Vector3Pool.Get();
         }

         if (useUVCoordinates) {
            uvs = Vector2Pool.Get();
         }
         if (useUV2Coordinates) {
            uvs2 = Vector2Pool.Get();
         }

         triangles = IntPool.Get();
      }

      public void Apply() {
         hexMesh.SetVertices(vertices);
         Vector3Pool.Add(vertices);
         if (useCellData) {
            hexMesh.SetColors(cellWeights);
            ColorPool.Add(cellWeights);
            hexMesh.SetUVs(2, cellIndices);
            Vector3Pool.Add(cellIndices);
         }

         if (useUVCoordinates) {
            hexMesh.SetUVs(0, uvs);
            Vector2Pool.Add(uvs);
         }
         if (useUV2Coordinates) {
            hexMesh.SetUVs(1, uvs2);
            Vector2Pool.Add(uvs2);
         }

         hexMesh.SetTriangles(triangles, 0);
         IntPool.Add(triangles);
         hexMesh.RecalculateNormals();

         if (useCollider) {
            meshCollider.sharedMesh = hexMesh;
         }
      }

      #region Triangles 

      public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
         int vertexIndex = vertices.Count;
         vertices.Add(HexMetrics.Perturb(v1));
         vertices.Add(HexMetrics.Perturb(v2));
         vertices.Add(HexMetrics.Perturb(v3));
         triangles.Add(vertexIndex);
         triangles.Add(vertexIndex + 1);
         triangles.Add(vertexIndex + 2);
      }

      public void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3) {
         int vertexIndex = vertices.Count;
         vertices.Add(v1);
         vertices.Add(v2);
         vertices.Add(v3);
         triangles.Add(vertexIndex);
         triangles.Add(vertexIndex + 1);
         triangles.Add(vertexIndex + 2);
      }

      public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector2 uv3) {
         uvs.Add(uv1);
         uvs.Add(uv2);
         uvs.Add(uv3);
      }

      public void AddTriangleUV2(Vector2 uv1, Vector2 uv2, Vector3 uv3) {
         uvs2.Add(uv1);
         uvs2.Add(uv2);
         uvs2.Add(uv3);
      }

      public void AddTriangleCellData(Vector3 indices, Color weights1, Color weights2, Color weights3) {
         cellIndices.Add(indices);
         cellIndices.Add(indices);
         cellIndices.Add(indices);
         cellWeights.Add(weights1);
         cellWeights.Add(weights2);
         cellWeights.Add(weights3);
      }

      public void AddTriangleCellData(Vector3 indices, Color weights) {
         AddTriangleCellData(indices, weights, weights, weights);
      }

      #endregion

      #region Quads

      /// <summary>
      /// Add a quad, applying perturbation to the positions.
      /// </summary>
      /// <param name="v1">First vertex position.</param>
      /// <param name="v2">Second vertex position.</param>
      /// <param name="v3">Third vertex position.</param>
      /// <param name="v4">Fourth vertex position.</param>
      public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
         int vertexIndex = vertices.Count;
         vertices.Add(HexMetrics.Perturb(v1));
         vertices.Add(HexMetrics.Perturb(v2));
         vertices.Add(HexMetrics.Perturb(v3));
         vertices.Add(HexMetrics.Perturb(v4));
         triangles.Add(vertexIndex);
         triangles.Add(vertexIndex + 2);
         triangles.Add(vertexIndex + 1);
         triangles.Add(vertexIndex + 1);
         triangles.Add(vertexIndex + 2);
         triangles.Add(vertexIndex + 3);
      }

      /// <summary>
      /// Add a quad verbatim, without perturbing the positions.
      /// </summary>
      /// <param name="v1">First vertex position.</param>
      /// <param name="v2">Second vertex position.</param>
      /// <param name="v3">Third vertex position.</param>
      /// <param name="v4">Fourth vertex position.</param>
      public void AddQuadUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
         int vertexIndex = vertices.Count;
         vertices.Add(v1);
         vertices.Add(v2);
         vertices.Add(v3);
         vertices.Add(v4);
         triangles.Add(vertexIndex);
         triangles.Add(vertexIndex + 2);
         triangles.Add(vertexIndex + 1);
         triangles.Add(vertexIndex + 1);
         triangles.Add(vertexIndex + 2);
         triangles.Add(vertexIndex + 3);
      }

      /// <summary>
      /// Add UV coordinates for a quad.
      /// </summary>
      /// <param name="uv1">First UV coordinates.</param>
      /// <param name="uv2">Second UV coordinates.</param>
      /// <param name="uv3">Third UV coordinates.</param>
      /// <param name="uv4">Fourth UV coordinates.</param>
      public void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4) {
         uvs.Add(uv1);
         uvs.Add(uv2);
         uvs.Add(uv3);
         uvs.Add(uv4);
      }

      /// <summary>
      /// Add UV coordaintes for a quad.
      /// </summary>
      /// <param name="uMin">Minimum U coordinate.</param>
      /// <param name="uMax">Maximum U coordinate.</param>
      /// <param name="vMin">Minimum V coordinate.</param>
      /// <param name="vMax">Maximum V coorindate.</param>
      public void AddQuadUV(float uMin, float uMax, float vMin, float vMax) {
         uvs.Add(new Vector2(uMin, vMin));
         uvs.Add(new Vector2(uMax, vMin));
         uvs.Add(new Vector2(uMin, vMax));
         uvs.Add(new Vector2(uMax, vMax));
      }

      /// <summary>
      /// Add UV2 coordinates for a quad.
      /// </summary>
      /// <param name="uv1">First UV2 coordinates.</param>
      /// <param name="uv2">Second UV2 coordinates.</param>
      /// <param name="uv3">Third UV2 coordinates.</param>
      /// <param name="uv4">Fourth UV2 coordinates.</param>
      public void AddQuadUV2(Vector2 uv1, Vector2 uv2, Vector3 uv3, Vector3 uv4) {
         uvs2.Add(uv1);
         uvs2.Add(uv2);
         uvs2.Add(uv3);
         uvs2.Add(uv4);
      }

      /// <summary>
      /// Add UV2 coordaintes for a quad.
      /// </summary>
      /// <param name="uMin">Minimum U2 coordinate.</param>
      /// <param name="uMax">Maximum U2 coordinate.</param>
      /// <param name="vMin">Minimum V2 coordinate.</param>
      /// <param name="vMax">Maximum V2 coorindate.</param>
      public void AddQuadUV2(float uMin, float uMax, float vMin, float vMax) {
         uvs2.Add(new Vector2(uMin, vMin));
         uvs2.Add(new Vector2(uMax, vMin));
         uvs2.Add(new Vector2(uMin, vMax));
         uvs2.Add(new Vector2(uMax, vMax));
      }

      /// <summary>
      /// Add cell data for a quad.
      /// </summary>
      /// <param name="indices">Terrain type indices.</param>
      /// <param name="weights1">First terrain weights.</param>
      /// <param name="weights2">Second terrain weights.</param>
      /// <param name="weights3">Third terrain weights.</param>
      /// <param name="weights4">Fourth terrain weights.</param>
      public void AddQuadCellData(Vector3 indices, Color weights1, Color weights2, Color weights3, Color weights4) {
         cellIndices.Add(indices);
         cellIndices.Add(indices);
         cellIndices.Add(indices);
         cellIndices.Add(indices);
         cellWeights.Add(weights1);
         cellWeights.Add(weights2);
         cellWeights.Add(weights3);
         cellWeights.Add(weights4);
      }

      /// <summary>
      /// Add cell data for a quad.
      /// </summary>
      /// <param name="indices">Terrain type indices.</param>
      /// <param name="weights1">First and second terrain weights, both the same.</param>
      /// <param name="weights2">Third and fourth terrain weights, both the same.</param>
      public void AddQuadCellData(Vector3 indices, Color weights1, Color weights2) =>
         AddQuadCellData(indices, weights1, weights1, weights2, weights2);

      /// <summary>
      /// Add cell data for a quad.
      /// </summary>
      /// <param name="indices">Terrain type indices.</param>
      /// <param name="weights">Terrain weights, uniform for entire quad.</param>
      public void AddQuadCellData(Vector3 indices, Color weights) =>
         AddQuadCellData(indices, weights, weights, weights, weights);

      #endregion
   }
}