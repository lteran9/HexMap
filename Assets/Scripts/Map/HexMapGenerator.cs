using System.Collections;
using System.Collections.Generic;
using HexMap.Misc;
using UnityEngine;
using HexMap.Map.Grid;

namespace HexMap.Map {
   public class HexMapGenerator : MonoBehaviour {
      struct MapRegion {
         public int xMin, xMax, zMin, zMax;

         public override string ToString() => $"{xMin}, {xMax}, {zMin}, {zMax}";
      }

      private int cellCount = 0;
      private int searchFrontierPhase;

      private List<MapRegion> regions;
      private HexCellPriorityQueue searchFrontier;

      [SerializeField] private bool _useFixedSeed = default;

      [SerializeField] private int _seed = 0;

      [Range(20, 200)]
      [SerializeField] private int _chunkSizeMin = 30;
      [Range(20, 200)]
      [SerializeField] private int _chunkSizeMax = 100;
      [Range(5, 95)]
      [SerializeField] private int _landPercentage = 50;
      [Range(1, 5)]
      [SerializeField] private int _waterLevel = 3;
      [Range(-4, 0)]
      [SerializeField] private int _elevationMinimum = -2;
      [Range(6, 10)]
      [SerializeField] private int _elevationMaximum = 8;
      [Range(0, 10)]
      [SerializeField] private int _mapBorderX = 5;
      [Range(0, 10)]
      [SerializeField] private int _mapBorderZ = 5;
      [Range(0, 10)]
      [SerializeField] private int _regionBorder = 5;
      [Range(1, 4)]
      [SerializeField] private int _regionCount = 1;
      [Range(0, 100)]
      [SerializeField] private int _erosionPercentage = 50;


      [Range(0, 0.5f)]
      [SerializeField] private float _jitterProbability = 0.25f;
      [Range(0f, 1f)]
      [SerializeField] private float _highRiseProbability = 0.25f;
      [Range(0f, 0.4f)]
      [SerializeField] private float sinkProbability = 0.2f;

      [SerializeField] private HexGrid _hexGrid;

      public void GenerateMap(int x, int z) {
         Debug.Log(nameof(GenerateMap));
         Random.State originalRandomState = Random.state;
         if (!_useFixedSeed) {
            _seed = Random.Range(0, int.MaxValue);
            _seed ^= (int)System.DateTime.Now.Ticks;
            _seed ^= (int)Time.unscaledTime;
            _seed &= int.MaxValue;
         }
         Random.InitState(_seed);

         cellCount = x * z;
         _hexGrid.CreateMap(x, z);
         if (searchFrontier == null) {
            searchFrontier = new HexCellPriorityQueue();
         }

         for (int i = 0; i < cellCount; i++) {
            _hexGrid.GetCell(i).WaterLevel = _waterLevel;
         }

         CreateRegions();
         CreateLand();
         ErodeLand();
         SetTerrainType();
         for (int i = 0; i < cellCount; i++) {
            _hexGrid.GetCell(i).SearchPhase = 0;
         }
         Random.state = originalRandomState;
      }

      private int RaiseTerrain(int chunkSize, int budget, MapRegion region) {
         searchFrontierPhase += 1;
         HexCell firstCell = GetRandomCell(region);
         firstCell.SearchPhase = searchFrontierPhase;
         firstCell.Distance = 0;
         firstCell.SearchHeuristic = 0;
         searchFrontier.Enqueue(firstCell);
         HexCoordinates center = firstCell.Coordinates;

         int rise = Random.value < _highRiseProbability ? 2 : 1;
         int size = 0;
         while (size < chunkSize && searchFrontier.Count > 0) {
            HexCell current = searchFrontier.Dequeue();
            int originalElevation = current.Elevation;
            int newElevation = originalElevation + rise;
            if (newElevation > _elevationMaximum) {
               continue;
            }
            current.Elevation = newElevation;
            if (originalElevation < _waterLevel && current.Elevation <= _waterLevel && --budget <= 0) {
               break;
            }
            size += 1;

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
               HexCell neighbor = current.GetNeighbor(d);
               if (neighbor && neighbor.SearchPhase < searchFrontierPhase) {
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

      private int SinkTerrain(int chunkSize, int budget, MapRegion region) {
         searchFrontierPhase += 1;
         HexCell firstCell = GetRandomCell(region);
         firstCell.SearchPhase = searchFrontierPhase;
         firstCell.Distance = 0;
         firstCell.SearchHeuristic = 0;
         searchFrontier.Enqueue(firstCell);
         HexCoordinates center = firstCell.Coordinates;

         int sink = Random.value < _highRiseProbability ? 2 : 1;
         int size = 0;
         while (size < chunkSize && searchFrontier.Count > 0) {
            HexCell current = searchFrontier.Dequeue();
            int originalElevation = current.Elevation;
            int newElevation = current.Elevation - sink;
            if (newElevation < _elevationMinimum) {
               continue;
            }
            current.Elevation = newElevation;
            if (originalElevation >= _waterLevel && newElevation < _waterLevel) {
               budget += 1;
            }
            size += 1;

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
               HexCell neighbor = current.GetNeighbor(d);
               if (neighbor && neighbor.SearchPhase < searchFrontierPhase) {
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

      private bool IsErodible(HexCell cell) {
         int erodibleElevation = cell.Elevation - 2;
         for (var d = HexDirection.NE; d <= HexDirection.NW; d++) {
            HexCell neighbor = cell.GetNeighbor(d);
            if (neighbor && neighbor.Elevation <= erodibleElevation) {
               return true;
            }
         }

         return false;
      }

      private void CreateLand() {
         int landBudget = Mathf.RoundToInt(cellCount * _landPercentage * 0.01f);
         for (int guard = 0; guard < 10000; guard++) {
            bool sink = Random.value < sinkProbability;
            for (int i = 0; i < regions.Count; i++) {
               MapRegion region = regions[i];
               int chunkSize = Random.Range(_chunkSizeMin, _chunkSizeMax - 1);
               if (sink) {
                  landBudget = SinkTerrain(chunkSize, landBudget, region);
               } else {
                  landBudget = RaiseTerrain(chunkSize, landBudget, region);
                  if (landBudget <= 0) {
                     return;
                  }
               }
               Debug.Log($"{guard}: Land Budget {landBudget}");
            }
         }

         if (landBudget > 0) {
            Debug.Log("Avoided an infinite loop.");
         }
      }

      private void ErodeLand() {
         List<HexCell> erodibleCells = ListPool<HexCell>.Get();
         for (int i = 0; i < cellCount; i++) {
            HexCell cell = _hexGrid.GetCell(i);
            if (IsErodible(cell)) {
               erodibleCells.Add(cell);
            }
         }

         int targetErodibleCount =
            (int)(erodibleCells.Count * (100 - _erosionPercentage) * 0.01f);

         while (erodibleCells.Count > targetErodibleCount) {
            int index = Random.Range(0, erodibleCells.Count);
            HexCell cell = erodibleCells[index];
            HexCell targetCell = GetErosionTarget(cell);

            cell.Elevation -= 1;
            targetCell.Elevation += 1;

            if (!IsErodible(cell)) {
               erodibleCells[index] = erodibleCells[erodibleCells.Count - 1];
               erodibleCells.RemoveAt(erodibleCells.Count - 1);
            }

            for (var d = HexDirection.NE; d <= HexDirection.NW; d++) {
               HexCell neighbor = cell.GetNeighbor(d);
               if (
                  neighbor && neighbor.Elevation == cell.Elevation + 2 &&
                  !erodibleCells.Contains(neighbor)
               ) {
                  erodibleCells.Add(neighbor);
               }
            }

            if (IsErodible(targetCell) && !erodibleCells.Contains(targetCell)) {
               erodibleCells.Add(targetCell);
            }

            for (var d = HexDirection.NE; d <= HexDirection.NW; d++) {
               HexCell neighbor = targetCell.GetNeighbor(d);
               if (
                  neighbor && neighbor != cell &&
                  neighbor.Elevation == targetCell.Elevation + 1 &&
                  !IsErodible(neighbor) &&
                  erodibleCells.Contains(neighbor)
               ) {
                  erodibleCells.Remove(neighbor);
               }
            }
         }

         ListPool<HexCell>.Add(erodibleCells);
      }

      private void CreateRegions() {
         if (regions == null) {
            regions = new List<MapRegion>();
         } else {
            regions.Clear();
         }

         MapRegion region;
         switch (_regionCount) {
            default:
               region.xMin = _mapBorderX;
               region.xMax = _hexGrid.GetCellCountX() - _mapBorderX;
               region.zMin = _mapBorderZ;
               region.zMax = _hexGrid.GetCellCountZ() - _mapBorderZ;
               regions.Add(region);
               break;
            case 2:
               if (Random.value < 0.5f) {

                  region.xMin = _mapBorderX;
                  region.xMax = _hexGrid.GetCellCountX() / 2 - _regionBorder;
                  region.zMin = _mapBorderZ;
                  region.zMax = _hexGrid.GetCellCountZ() - _mapBorderZ;
                  regions.Add(region);
                  region.xMin = _hexGrid.GetCellCountX() / 2 + _regionBorder;
                  region.xMax = _hexGrid.GetCellCountX() - _mapBorderX;
                  regions.Add(region);
               } else {
                  region.xMin = _mapBorderX;
                  region.xMax = _hexGrid.GetCellCountX() - _mapBorderX;
                  region.zMin = _mapBorderZ;
                  region.zMax = _hexGrid.GetCellCountZ() / 2 - _regionBorder;
                  regions.Add(region);
                  region.zMin = _hexGrid.GetCellCountZ() / 2 + _regionBorder;
                  region.zMax = _hexGrid.GetCellCountZ() - _mapBorderZ;
                  regions.Add(region);
               }
               break;
            case 3:
               region.xMin = _mapBorderX;
               region.xMax = _hexGrid.GetCellCountX() / 3 - _regionBorder;
               region.zMin = _mapBorderZ;
               region.zMax = _hexGrid.GetCellCountZ() - _mapBorderZ;
               regions.Add(region);
               region.xMin = _hexGrid.GetCellCountX() / 3 + _regionBorder;
               region.xMax = _hexGrid.GetCellCountX() * 2 / 3 - _regionBorder;
               regions.Add(region);
               region.xMin = _hexGrid.GetCellCountX() * 2 / 3 + _regionBorder;
               region.xMax = _hexGrid.GetCellCountX() - _mapBorderX;
               break;
            case 4:
               region.xMin = _mapBorderX;
               region.xMax = _hexGrid.GetCellCountX() / 2 - _regionBorder;
               region.zMin = _mapBorderZ;
               region.zMax = _hexGrid.GetCellCountZ() / 2 - _regionBorder;
               regions.Add(region);
               region.xMin = _hexGrid.GetCellCountX() / 2 + _regionBorder;
               region.xMax = _hexGrid.GetCellCountX() - _mapBorderX;
               regions.Add(region);
               region.zMin = _hexGrid.GetCellCountZ() / 2 + _regionBorder;
               region.zMax = _hexGrid.GetCellCountZ() - _mapBorderZ;
               regions.Add(region);
               region.xMin = _mapBorderX;
               region.xMax = _hexGrid.GetCellCountX() / 2 - _regionBorder;
               regions.Add(region);
               break;
         }
      }

      private void SetTerrainType() {
         for (int i = 0; i < cellCount; i++) {
            HexCell cell = _hexGrid.GetCell(i);
            if (!cell.IsUnderwater) {
               cell.TerrainTypeIndex = cell.Elevation - cell.WaterLevel;
            }
         }
      }

      private HexCell GetErosionTarget(HexCell cell) {
         List<HexCell> candidates = ListPool<HexCell>.Get();
         int erodibleElevation = cell.Elevation - 2;
         for (var d = HexDirection.NE; d <= HexDirection.NW; d++) {
            HexCell neighbor = cell.GetNeighbor(d);
            if (neighbor && neighbor.Elevation <= erodibleElevation) {
               candidates.Add(neighbor);
            }
         }
         HexCell target = candidates[Random.Range(0, candidates.Count)];
         ListPool<HexCell>.Add(candidates);
         return target;
      }

      private HexCell GetRandomCell(MapRegion region) {
         return _hexGrid.GetCell(Random.Range(region.xMin, region.xMax), Random.Range(region.zMin, region.zMax));
      }
   }
}
