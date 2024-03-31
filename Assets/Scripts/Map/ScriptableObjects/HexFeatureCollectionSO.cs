using System;
using HexMap.Map.Grid;
using UnityEngine;

namespace HexMap.Map.ScriptableObjects {
   [CreateAssetMenu(fileName = "Collection", menuName = "Map/FeatureCollection")]
   public class HexFeatureCollectionSO : ScriptableObject {
      [SerializeField] private HexFeatureCollection[] collection = default;

      public Transform Pick(int index, float choice) {
         return collection[index].Pick(choice);
      }
   }
}