using HexMap.Misc;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Map
{
   [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
   public class HexMesh : MonoBehaviour
   {
      #region Buffers

      [NonSerialized]
      List<int> Triangles = default;

      [NonSerialized]
      List<Color> cellWeights = default;

      [NonSerialized]
      List<Vector2> UVs = default,
         UV2s = default;

      [NonSerialized]
      List<Vector3> Vertices = default,
         cellIndices = default;

      #endregion

      Mesh hexMesh = default;
      MeshRenderer meshRenderer = default;
      MeshCollider meshCollider = default;

      public bool useCollider,
         useCellData,
         useUVCoordinates,
         useUV2Coordinates;

      void Awake()
      {
         GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
         meshRenderer = GetComponent<MeshRenderer>();
         if (useCollider)
         {
            meshCollider = gameObject.AddComponent<MeshCollider>();
         }
         hexMesh.name = "Hex Mesh";
      }

      public void Clear()
      {
         hexMesh.Clear();
         Vertices = ListPool<Vector3>.Get();

         if (useCellData)
         {
            cellWeights = ListPool<Color>.Get();
            cellIndices = ListPool<Vector3>.Get();
         }

         if (useUVCoordinates)
         {
            UVs = ListPool<Vector2>.Get();
         }
         if (useUV2Coordinates)
         {
            UV2s = ListPool<Vector2>.Get();
         }

         Triangles = ListPool<int>.Get();
      }

      public void Apply()
      {
         hexMesh.SetVertices(Vertices);
         ListPool<Vector3>.Add(Vertices);
         if (useCellData)
         {
            hexMesh.SetColors(cellWeights);
            ListPool<Color>.Add(cellWeights);
            hexMesh.SetUVs(2, cellIndices);
            ListPool<Vector3>.Add(cellIndices);
         }

         if (useUVCoordinates)
         {
            hexMesh.SetUVs(0, UVs);
            ListPool<Vector2>.Add(UVs);
         }
         if (useUV2Coordinates)
         {
            hexMesh.SetUVs(1, UV2s);
            ListPool<Vector2>.Add(UV2s);
         }

         hexMesh.SetTriangles(Triangles, 0);
         ListPool<int>.Add(Triangles);
         hexMesh.RecalculateNormals();

         if (useCollider)
         {
            meshCollider.sharedMesh = hexMesh;
         }
      }

      #region Triangles 

      public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
      {
         int vertexIndex = Vertices.Count;
         Vertices.Add(HexMetrics.Perturb(v1));
         Vertices.Add(HexMetrics.Perturb(v2));
         Vertices.Add(HexMetrics.Perturb(v3));
         Triangles.Add(vertexIndex);
         Triangles.Add(vertexIndex + 1);
         Triangles.Add(vertexIndex + 2);
      }

      public void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3)
      {
         int vertexIndex = Vertices.Count;
         Vertices.Add(v1);
         Vertices.Add(v2);
         Vertices.Add(v3);
         Triangles.Add(vertexIndex);
         Triangles.Add(vertexIndex + 1);
         Triangles.Add(vertexIndex + 2);
      }

      public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector2 uv3)
      {
         UVs.Add(uv1);
         UVs.Add(uv2);
         UVs.Add(uv3);
      }

      public void AddTriangleUV2(Vector2 uv1, Vector2 uv2, Vector3 uv3)
      {
         UV2s.Add(uv1);
         UV2s.Add(uv2);
         UV2s.Add(uv3);
      }

      public void AddTriangleCellData(Vector3 indices, Color weights1, Color weights2, Color weights3)
      {
         cellIndices.Add(indices);
         cellIndices.Add(indices);
         cellIndices.Add(indices);
         cellWeights.Add(weights1);
         cellWeights.Add(weights2);
         cellWeights.Add(weights3);
      }

      public void AddTriangleCellData(Vector3 indices, Color weights)
      {
         AddTriangleCellData(indices, weights, weights, weights);
      }

      #endregion

      #region Quads

      public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
      {
         int vertexIndex = Vertices.Count;
         Vertices.Add(HexMetrics.Perturb(v1));
         Vertices.Add(HexMetrics.Perturb(v2));
         Vertices.Add(HexMetrics.Perturb(v3));
         Vertices.Add(HexMetrics.Perturb(v4));
         Triangles.Add(vertexIndex);
         Triangles.Add(vertexIndex + 2);
         Triangles.Add(vertexIndex + 1);
         Triangles.Add(vertexIndex + 1);
         Triangles.Add(vertexIndex + 2);
         Triangles.Add(vertexIndex + 3);
      }

      public void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4)
      {
         UVs.Add(uv1);
         UVs.Add(uv2);
         UVs.Add(uv3);
         UVs.Add(uv4);
      }

      public void AddQuadUV(float uMin, float uMax, float vMin, float vMax)
      {
         UVs.Add(new Vector2(uMin, vMin));
         UVs.Add(new Vector2(uMax, vMin));
         UVs.Add(new Vector2(uMin, vMax));
         UVs.Add(new Vector2(uMax, vMax));
      }

      public void AddQuadUV2(Vector2 uv1, Vector2 uv2, Vector3 uv3, Vector3 uv4)
      {
         UV2s.Add(uv1);
         UV2s.Add(uv2);
         UV2s.Add(uv3);
         UV2s.Add(uv4);
      }

      public void AddQuadUV2(float uMin, float uMax, float vMin, float vMax)
      {
         UV2s.Add(new Vector2(uMin, vMin));
         UV2s.Add(new Vector2(uMax, vMin));
         UV2s.Add(new Vector2(uMin, vMax));
         UV2s.Add(new Vector2(uMax, vMax));
      }

      public void AddQuadUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
      {
         int vertexIndex = Vertices.Count;
         Vertices.Add(v1);
         Vertices.Add(v2);
         Vertices.Add(v3);
         Vertices.Add(v4);
         Triangles.Add(vertexIndex);
         Triangles.Add(vertexIndex + 2);
         Triangles.Add(vertexIndex + 1);
         Triangles.Add(vertexIndex + 1);
         Triangles.Add(vertexIndex + 2);
         Triangles.Add(vertexIndex + 3);
      }

      public void AddQuadCellData(Vector3 indices, Color weights1, Color weights2, Color weights3, Color weights4)
      {
         cellIndices.Add(indices);
         cellIndices.Add(indices);
         cellIndices.Add(indices);
         cellIndices.Add(indices);
         cellWeights.Add(weights1);
         cellWeights.Add(weights2);
         cellWeights.Add(weights3);
         cellWeights.Add(weights4);
      }

      public void AddQuadCellData(Vector3 indices, Color weights1, Color weights2)
      {
         AddQuadCellData(indices, weights1, weights1, weights2, weights2);
      }

      public void AddQuadCellData(Vector3 indices, Color weights)
      {
         AddQuadCellData(indices, weights, weights, weights, weights);
      }

      #endregion
   }
}