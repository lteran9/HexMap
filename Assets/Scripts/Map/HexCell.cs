using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Map
{
   public class HexCell : MonoBehaviour
   {
      [NonSerialized]
      public HexCoordinates coordinates = default;
      [NonSerialized]
      public Color color;
   }
}