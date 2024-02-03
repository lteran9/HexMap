using System.IO;
using UnityEngine;

namespace HexMap.Map {
   [System.Serializable]
   public struct HexCoordinates {
      #region Static Methods 

      public static HexCoordinates FromOffsetCoordinates(int x, int z) {
         return new HexCoordinates(x - z / 2, z);
      }

      public static HexCoordinates FromPosition(Vector3 position) {
         float x = position.x / (HexMetrics.InnerRadius * 2f);
         float y = -x;
         float offset = position.z / (HexMetrics.OuterRadius * 3f);
         x -= offset;
         y -= offset;

         int iX = Mathf.RoundToInt(x);
         int iY = Mathf.RoundToInt(y);
         int iZ = Mathf.RoundToInt(-x - y);

         if (iX + iY + iZ != 0) {
            float dX = Mathf.Abs(x - iX);
            float dY = Mathf.Abs(y - iY);
            float dZ = Mathf.Abs(-x - y - iZ);

            if (dX > dY && dX > dZ) {
               iX = -iY - iZ;
            } else if (dZ > dY) {
               iZ = -iX - iY;
            }
         }

         return new HexCoordinates(iX, iZ);
      }

      public static HexCoordinates Load(BinaryReader reader) {
         HexCoordinates c;
         c.x = reader.ReadInt32();
         c.z = reader.ReadInt32();
         return c;
      }

      #endregion

      [SerializeField] private int x, z;

      public int X { get { return x; } }
      public int Z { get { return z; } }
      public int Y {
         get {
            return -X - Z;
         }
      }

      public HexCoordinates(int x, int z) {
         this.x = x;
         this.z = z;
      }

      public override string ToString() {
         return "(" + X.ToString() + "," + Z.ToString() + ")";
      }

      public string ToStringOnSeparateLines() {
         return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
      }

      public int DistanceTo(HexCoordinates other) {
         return
            ((X < other.X ? other.X - X : X - other.X) +
            (Y < other.Y ? other.Y - Y : Y - other.Y) +
            (Z < other.Z ? other.Z - Z : Z - other.Z)) / 2;
      }

      public void Save(BinaryWriter writer) {
         writer.Write(x);
         writer.Write(z);
      }
   }
}