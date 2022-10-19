using HexMap.Map;
using System.IO;
using UnityEngine;

namespace HexMap.Units
{
   public class HexUnit : MonoBehaviour
   {
      #region Static Methods

      public static HexUnit unitPrefab;

      public static void Load(BinaryReader reader, HexGrid grid)
      {
         HexCoordinates coordinates = HexCoordinates.Load(reader);
         float orientation = reader.ReadSingle();
         grid.AddUnit(
            Instantiate(unitPrefab), grid.GetCell(coordinates), orientation
         );
      }

      #endregion

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

      public HexCell Location
      {
         get
         {
            return location;
         }
         set
         {
            if (location)
            {
               location.Unit = null;
            }
            location = value;
            value.Unit = this;
            transform.localPosition = value.Position;
         }
      }

      float orientation = default;
      HexCell location = default;

      public void ValidateLocation()
      {
         transform.localPosition = location.Position;
      }

      public void Die()
      {
         location.Unit = null;
         Destroy(gameObject);
      }

      public void Save(BinaryWriter writer)
      {
         location.Coordinates.Save(writer);
         writer.Write(orientation);
      }

      public bool IsValidDestination(HexCell cell)
      {
         return !cell.IsUnderwater && !cell.Unit;
      }
   }
}