using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Map {
   public struct HexHash {
      public float a { get; private set; }
      public float b { get; private set; }
      public float c { get; private set; }
      public float d { get; private set; }
      public float e { get; private set; }

      public static HexHash Create() {
         var hash = new HexHash();
         hash.a = Random.value * 0.999f;
         hash.b = Random.value * 0.999f;
         hash.c = Random.value * 0.999f;
         hash.d = Random.value * 0.999f;
         hash.e = Random.value * 0.999f;
         return hash;
      }
   }
}
