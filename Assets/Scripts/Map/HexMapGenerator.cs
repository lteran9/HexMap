using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Map
{
   public class HexMapGenerator : MonoBehaviour
   {
      int cellCount = 0;
      int searchFrontierPhase;

      HexCellPriorityQueue searchFrontier;

      [Range(20, 200)]
      [SerializeField] int _chunkSizeMin = 30;
      [Range(20, 200)]
      [SerializeField] int _chunkSizeMax = 100;
      [Range(5, 95)]
      [SerializeField] int _landPercentage = 50;
      [Range(1, 5)]
      [SerializeField] int _waterLevel = 3;
      [Range(-4, 0)]
      [SerializeField] int _elevationMinimum = -2;
      [Range(6, 10)]
      [SerializeField] int _elevationMaximum = 8;

      [Range(0, 0.5f)]
      [SerializeField] float _jitterProbability = 0.25f;
      [Range(0f, 1f)]
      [SerializeField] float _highRiseProbability = 0.25f;
      [Range(0f, 0.4f)]
      [SerializeField] float sinkProbability = 0.2f;

      [SerializeField] HexGrid _hexGrid;

      public void GenerateMap(int x, int z)
      {
         cellCount = x * z;
         _hexGrid.CreateMap(x, z);
         if (searchFrontier == null)
         {
            searchFrontier = new HexCellPriorityQueue();
         }
         for (int i = 0; i < cellCount; i++)
         {
            _hexGrid.GetCell(i).WaterLevel = _waterLevel;
         }
         CreateLand();
         SetTerrainType();
         for (int i = 0; i < cellCount; i++)
         {
            _hexGrid.GetCell(i).SearchPhase = 0;
         }
      }

      int RaiseTerrain(int chunkSize, int budget)
      {
         searchFrontierPhase += 1;
         HexCell firstCell = GetRandomCell();
         firstCell.SearchPhase = searchFrontierPhase;
         firstCell.Distance = 0;
         firstCell.SearchHeuristic = 0;
         searchFrontier.Enqueue(firstCell);
         HexCoordinates center = firstCell.Coordinates;

         int rise = Random.value < _highRiseProbability ? 2 : 1;
         int size = 0;
         while (size < chunkSize && searchFrontier.Count > 0)
         {
            HexCell current = searchFrontier.Dequeue();
            int originalElevation = current.Elevation;
            int newElevation = originalElevation + rise;
            if (newElevation > _elevationMaximum)
            {
               continue;
            }
            current.Elevation = newElevation;
            budget -= 1; // --budget in if statement was causing infinite loop
            if (originalElevation < _waterLevel && current.Elevation == _waterLevel && budget == 0)
            {
               break;
            }
            size += 1;

            for (HexGrid.HexDirection d = HexGrid.HexDirection.NE; d <= HexGrid.HexDirection.NW; d++)
            {
               HexCell neighbor = current.GetNeighbor(d);
               if (neighbor && neighbor.SearchPhase < searchFrontierPhase)
               {
                  neighbor.SearchPhase = searchFrontierPhase;
                  neighbor.Distance = neighbor.Coordinates.DistanceTo(center); ;
                  neighbor.SearchHeuristic = Random.value < _jitterProbability ? 1 : 0; ;
                  searchFrontier.Enqueue(neighbor);
               }
            }
         }
         searchFrontier.Clear();

         return budget;
      }

      int SinkTerrain(int chunkSize, int budget)
      {
         searchFrontierPhase += 1;
         HexCell firstCell = GetRandomCell();
         firstCell.SearchPhase = searchFrontierPhase;
         firstCell.Distance = 0;
         firstCell.SearchHeuristic = 0;
         searchFrontier.Enqueue(firstCell);
         HexCoordinates center = firstCell.Coordinates;

         int sink = Random.value < _highRiseProbability ? 2 : 1;
         int size = 0;
         while (size < chunkSize && searchFrontier.Count > 0)
         {
            HexCell current = searchFrontier.Dequeue();
            int originalElevation = current.Elevation;
            int newElevation = current.Elevation - sink;
            if (newElevation < _elevationMinimum)
            {
               continue;
            }
            current.Elevation = newElevation;
            if (originalElevation >= _waterLevel && newElevation < _waterLevel)
            {
               budget += 1;
            }
            size += 1;

            for (HexGrid.HexDirection d = HexGrid.HexDirection.NE; d <= HexGrid.HexDirection.NW; d++)
            {
               HexCell neighbor = current.GetNeighbor(d);
               if (neighbor && neighbor.SearchPhase < searchFrontierPhase)
               {
                  neighbor.SearchPhase = searchFrontierPhase;
                  neighbor.Distance = neighbor.Coordinates.DistanceTo(center); ;
                  neighbor.SearchHeuristic = Random.value < _jitterProbability ? 1 : 0; ;
                  searchFrontier.Enqueue(neighbor);
               }
            }
         }
         searchFrontier.Clear();

         return budget;
      }


      void CreateLand()
      {
         int landBudget = Mathf.RoundToInt(cellCount * _landPercentage * 0.01f);
         while (landBudget > 0)
         {
            int chunkSize = Random.Range(_chunkSizeMin, _chunkSizeMax + 1);
            if (Random.value < sinkProbability)
            {
               landBudget = SinkTerrain(chunkSize, landBudget);
            }
            else
            {
               landBudget = RaiseTerrain(chunkSize, landBudget);
            }
         }
      }

      void SetTerrainType()
      {
         for (int i = 0; i < cellCount; i++)
         {
            HexCell cell = _hexGrid.GetCell(i);
            if (!cell.IsUnderwater)
            {
               cell.TerrainTypeIndex = cell.Elevation - cell.WaterLevel;
            }
         }
      }

      HexCell GetRandomCell()
      {
         return _hexGrid.GetCell(Random.Range(0, cellCount));
      }
   }
}
