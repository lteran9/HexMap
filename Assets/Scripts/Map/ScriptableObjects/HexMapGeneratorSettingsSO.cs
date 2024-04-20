using System;
using UnityEngine;

namespace HexMap.Map.ScriptableObjects {
   [CreateAssetMenu(fileName = "MapGeneratorSettings", menuName = "Map/HexMapGenerator")]
   public class HexMapGeneratorSettingsSO : ScriptableObject {
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
      [SerializeField] private float _sinkProbability = 0.2f;

      public bool UseFixedSeed => _useFixedSeed;
      public int Seed => _seed;
      public int ChunkSizeMin => _chunkSizeMin;
      public int ChunkSizeMax => _chunkSizeMax;
      public int LandPercentage => _landPercentage;
      public int WaterLevel => _waterLevel;
      public int ElevationMaximum => _elevationMaximum;
      public int EleveationMinimum => _elevationMinimum;
      public int MapBorderX => _mapBorderX;
      public int MapBorderZ => _mapBorderZ;
      public int RegionBorder => _regionBorder;
      public int RegionCount => _regionCount;
      public int ErosionPercentage => _erosionPercentage;
      public float JitterProbability => _jitterProbability;
      public float HighRiseProbability => _highRiseProbability;
      public float SinkProbability => _sinkProbability;
   }
}