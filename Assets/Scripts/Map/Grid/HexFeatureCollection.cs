using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Map.Grid {
   [System.Serializable]
   public struct HexFeatureCollection {
      public Transform[] prefabs;

      public Transform Pick(float choice) {
         return prefabs[(int)(choice * prefabs.Length)];
      }
   }
}