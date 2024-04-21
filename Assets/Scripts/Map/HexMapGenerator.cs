using System.Collections;
using System.Collections.Generic;
using HexMap.Misc;
using UnityEngine;
using HexMap.Map.Grid;
using HexMap.Map.ScriptableObjects;

namespace HexMap.Map {
   public class HexMapGenerator : MonoBehaviour {
      private struct MapRegion {
         public int xMin, xMax, zMin, zMax;
      }

      [SerializeField] private HexMapGeneratorSettingsSO _generatorSettings = default;

      private int cellCount = 0;
      private int searchFrontierPhase = 0;

      private HexGrid _hexGrid = default;
      private HexCellPriorityQueue searchFrontier = default;
      private List<MapRegion> regions = default;

      private void Awake() {
         _hexGrid = GetComponentInParent<HexGrid>();
      }

      private void Start() {
         GenerateMap(20, 15);
      }

      public void GenerateMap(int x, int z) {
         Random.State originalRandomState = Random.state;
         if (_generatorSettings.UseFixedSeed) {
            Random.InitState(_generatorSettings.Seed);
         } else {
            var fixedSeed = Random.Range(0, int.MaxValue);
            fixedSeed ^= (int)System.DateTime.Now.Ticks;
            fixedSeed ^= (int)Time.unscaledTime;
            fixedSeed &= int.MaxValue;
            Random.InitState(fixedSeed);
         }

         cellCount = x * z;
         _hexGrid.CreateMap(x, z);
         if (searchFrontier == null) {
            searchFrontier = new HexCellPriorityQueue();
         }

         for (int i = 0; i < cellCount; i++) {
            _hexGrid.GetCell(i).WaterLevel = _generatorSettings.WaterLevel;
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

         int rise = Random.value < _generatorSettings.HighRiseProbability ? 2 : 1;
         int size = 0;
         while (size < chunkSize && searchFrontier.Count > 0) {
            HexCell current = searchFrontier.Dequeue();
            int originalElevation = current.Elevation;
            int newElevation = originalElevation + rise;
            if (newElevation > _generatorSettings.ElevationMaximum) {
               continue;
            }
            current.Elevation = newElevation;
            if (originalElevation < _generatorSettings.WaterLevel && current.Elevation <= _generatorSettings.WaterLevel && --budget <= 0) {
               break;
            }
            size += 1;

            for (HexGridDirection d = HexGridDirection.NE; d <= HexGridDirection.NW; d++) {
               HexCell neighbor = current.GetNeighbor(d);
               if (neighbor && neighbor.SearchPhase < searchFrontierPhase) {
                  neighbor.SearchPhase = searchFrontierPhase;
                  neighbor.Distance = neighbor.Coordinates.DistanceTo(center); ;
                  neighbor.SearchHeuristic = Random.value < _generatorSettings.JitterProbability ? 1 : 0; ;
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

         int sink = Random.value < _generatorSettings.HighRiseProbability ? 2 : 1;
         int size = 0;
         while (size < chunkSize && searchFrontier.Count > 0) {
            HexCell current = searchFrontier.Dequeue();
            int originalElevation = current.Elevation;
            int newElevation = current.Elevation - sink;
            if (newElevation < _generatorSettings.EleveationMinimum) {
               continue;
            }
            current.Elevation = newElevation;
            if (originalElevation >= _generatorSettings.WaterLevel && newElevation < _generatorSettings.WaterLevel) {
               budget += 1;
            }
            size += 1;

            for (HexGridDirection d = HexGridDirection.NE; d <= HexGridDirection.NW; d++) {
               HexCell neighbor = current.GetNeighbor(d);
               if (neighbor && neighbor.SearchPhase < searchFrontierPhase) {
                  neighbor.SearchPhase = searchFrontierPhase;
                  neighbor.Distance = neighbor.Coordinates.DistanceTo(center); ;
                  neighbor.SearchHeuristic = Random.value < _generatorSettings.JitterProbability ? 1 : 0; ;
                  searchFrontier.Enqueue(neighbor);
               }
            }
         }
         searchFrontier.Clear();

         return budget;
      }

      private bool IsErodible(HexCell cell) {
         int erodibleElevation = cell.Elevation - 2;
         for (var d = HexGridDirection.NE; d <= HexGridDirection.NW; d++) {
            HexCell neighbor = cell.GetNeighbor(d);
            if (neighbor && neighbor.Elevation <= erodibleElevation) {
               return true;
            }
         }

         return false;
      }

      private void CreateLand() {
         int landBudget = Mathf.RoundToInt(cellCount * _generatorSettings.LandPercentage * 0.01f);
         for (int guard = 0; guard < 10000; guard++) {
            bool sink = Random.value < _generatorSettings.SinkProbability;
            for (int i = 0; i < regions.Count; i++) {
               MapRegion region = regions[i];
               int chunkSize = Random.Range(_generatorSettings.ChunkSizeMin, _generatorSettings.ChunkSizeMax - 1);
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
         List<HexCell> erodibleCells = HexCellPool.Get();
         for (int i = 0; i < cellCount; i++) {
            HexCell cell = _hexGrid.GetCell(i);
            if (IsErodible(cell)) {
               erodibleCells.Add(cell);
            }
         }

         int targetErodibleCount =
            (int)(erodibleCells.Count * (100 - _generatorSettings.ErosionPercentage) * 0.01f);

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

            for (var d = HexGridDirection.NE; d <= HexGridDirection.NW; d++) {
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

            for (var d = HexGridDirection.NE; d <= HexGridDirection.NW; d++) {
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

         HexCellPool.Add(erodibleCells);
      }

      private void CreateRegions() {
         if (regions == null) {
            regions = new List<MapRegion>();
         } else {
            regions.Clear();
         }

         var region = new MapRegion();
         switch (_generatorSettings.RegionCount) {
            default:
               region.xMin = _generatorSettings.MapBorderX;
               region.xMax = _hexGrid.GetCellCountX() - _generatorSettings.MapBorderX;
               region.zMin = _generatorSettings.MapBorderZ;
               region.zMax = _hexGrid.GetCellCountZ() - _generatorSettings.MapBorderZ;
               regions.Add(region);
               break;
            case 2:
               if (Random.value < 0.5f) {
                  region.xMin = _generatorSettings.MapBorderX;
                  region.xMax = _hexGrid.GetCellCountX() / 2 - _generatorSettings.RegionBorder;
                  region.zMin = _generatorSettings.MapBorderZ;
                  region.zMax = _hexGrid.GetCellCountZ() - _generatorSettings.MapBorderZ;
                  regions.Add(region);
                  region.xMin = _hexGrid.GetCellCountX() / 2 + _generatorSettings.RegionBorder;
                  region.xMax = _hexGrid.GetCellCountX() - _generatorSettings.MapBorderX;
                  regions.Add(region);
               } else {
                  region.xMin = _generatorSettings.MapBorderX;
                  region.xMax = _hexGrid.GetCellCountX() - _generatorSettings.MapBorderX;
                  region.zMin = _generatorSettings.MapBorderZ;
                  region.zMax = _hexGrid.GetCellCountZ() / 2 - _generatorSettings.RegionBorder;
                  regions.Add(region);
                  region.zMin = _hexGrid.GetCellCountZ() / 2 + _generatorSettings.RegionBorder;
                  region.zMax = _hexGrid.GetCellCountZ() - _generatorSettings.MapBorderZ;
                  regions.Add(region);
               }
               break;
            case 3:
               region.xMin = _generatorSettings.MapBorderX;
               region.xMax = _hexGrid.GetCellCountX() / 3 - _generatorSettings.RegionBorder;
               region.zMin = _generatorSettings.MapBorderZ;
               region.zMax = _hexGrid.GetCellCountZ() - _generatorSettings.MapBorderZ;
               regions.Add(region);
               region.xMin = _hexGrid.GetCellCountX() / 3 + _generatorSettings.RegionBorder;
               region.xMax = _hexGrid.GetCellCountX() * 2 / 3 - _generatorSettings.RegionBorder;
               regions.Add(region);
               region.xMin = _hexGrid.GetCellCountX() * 2 / 3 + _generatorSettings.RegionBorder;
               region.xMax = _hexGrid.GetCellCountX() - _generatorSettings.MapBorderX;
               regions.Add(region);
               break;
            case 4:
               region.xMin = _generatorSettings.MapBorderX;
               region.xMax = _hexGrid.GetCellCountX() / 2 - _generatorSettings.RegionBorder;
               region.zMin = _generatorSettings.MapBorderZ;
               region.zMax = _hexGrid.GetCellCountZ() / 2 - _generatorSettings.RegionBorder;
               regions.Add(region);
               region.xMin = _hexGrid.GetCellCountX() / 2 + _generatorSettings.RegionBorder;
               region.xMax = _hexGrid.GetCellCountX() - _generatorSettings.MapBorderX;
               regions.Add(region);
               region.zMin = _hexGrid.GetCellCountZ() / 2 + _generatorSettings.RegionBorder;
               region.zMax = _hexGrid.GetCellCountZ() - _generatorSettings.MapBorderZ;
               regions.Add(region);
               region.xMin = _generatorSettings.MapBorderX;
               region.xMax = _hexGrid.GetCellCountX() / 2 - _generatorSettings.RegionBorder;
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
         List<HexCell> candidates = HexCellPool.Get();
         int erodibleElevation = cell.Elevation - 2;
         for (var d = HexGridDirection.NE; d <= HexGridDirection.NW; d++) {
            HexCell neighbor = cell.GetNeighbor(d);
            if (neighbor && neighbor.Elevation <= erodibleElevation) {
               candidates.Add(neighbor);
            }
         }
         HexCell target = candidates[Random.Range(0, candidates.Count)];
         HexCellPool.Add(candidates);
         return target;
      }

      private HexCell GetRandomCell(MapRegion region) {
         return _hexGrid.GetCell(Random.Range(region.xMin, region.xMax), Random.Range(region.zMin, region.zMax));
      }
   }
}
