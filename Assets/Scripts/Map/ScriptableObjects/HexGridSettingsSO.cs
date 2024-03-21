using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HexMap.Units;

namespace HexMap.Map {
   [CreateAssetMenu(fileName = "HexGridSettingsSO", menuName = "Map/HexGridSettings")]
   public class HexGridSettingsSO : ScriptableObject {
      [SerializeField] private int _seed = 0;
      [SerializeField, Tooltip("Must be multiple of 5.")] private int _cellCountX = 20;
      [SerializeField, Tooltip("Must be multiple of 5.")] private int _cellCountZ = 15;
      [SerializeField] private HexCell _cellPrefab = default;
      [SerializeField] private HexUnit _unitPrefab = default;
      [SerializeField] private HexGridChunk _chunkPrefab = default;
      [SerializeField] private TextMeshProUGUI _cellLabelPrefab = default;
      [SerializeField] private Texture2D _noiseSource = default;

      public int Seed => _seed;
      public int CellCountX => _cellCountX;
      public int CellCountZ => _cellCountZ;
      public HexCell CellPrefab => _cellPrefab;
      public HexUnit UnitPrefab => _unitPrefab;
      public HexGridChunk ChunkPrefab => _chunkPrefab;
      public TextMeshProUGUI CellLabelPrefab => _cellLabelPrefab;
      public Texture2D NoiseSource => _noiseSource;

      public void UpdateCellCountX(int x) {
         _cellCountX = x;
      }

      public void UpdatecellCoundZ(int z) {
         _cellCountZ = z;
      }
   }
}