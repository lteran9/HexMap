using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexMap.Map;

namespace HexMap.Units
{
   public class HexUnit : MonoBehaviour
   {
      public HexCell Location
      {
         get
         {
            return location;
         }
         set
         {
            location = value;
            value.Unit = this;
            transform.localPosition = value.Position;
         }
      }

      public float Orientation
      {
         get
         {
            return orientation;
         }
         set
         {
            orientation = value;
            transform.localRotation = Quaternion.Euler(0f, value, 0f);
         }
      }

      float orientation = default;
      HexCell location = default;

      public void ValidateLocation()
      {
         transform.localPosition = location.Position;
      }
   }
}