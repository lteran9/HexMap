using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Map {
   public class HexHash {
      public readonly float a;
      public readonly float b;
      public readonly float c;
      public readonly float d;
      public readonly float e;

      public HexHash() {
         a = Random.value * 0.999f;
         b = Random.value * 0.999f;
         c = Random.value * 0.999f;
         d = Random.value * 0.999f;
         e = Random.value * 0.999f;
      }
   }
}
