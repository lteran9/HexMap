using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using HexMap.Map;
using HexMap.Misc;
using HexMap.Map.Grid;

namespace HexMap.Units {
   public class HexUnit : MonoBehaviour {
      #region Static Methods

      private const float travelSpeed = 4f;
      private const float rotationSpeed = 180f;

      public static HexUnit unitPrefab = default;

      public static void Load(BinaryReader reader, HexGrid grid) {
         var coordinates = HexCoordinates.Load(reader);
         float orientation = reader.ReadSingle();
         grid.AddUnit(
            Instantiate(unitPrefab), grid.GetCell(coordinates), orientation
         );
      }

      #endregion

      private float orientation = default;

      private HexCell location = default;
      private HexCell currentTravelLocation = default;
      private HexGrid hexGrid = default;
      private List<HexCell> pathToTravel = default;

      public int Speed {
         get {
            return 24;
         }
      }

      public int VisionRange {
         get {
            return 2;
         }
      }

      public float Orientation {
         get {
            return orientation;
         }
         set {
            orientation = value;
            transform.localRotation = Quaternion.Euler(0f, value, 0f);
         }
      }

      public HexCell Location {
         get {
            return location;
         }
         set {
            if (location) {
               hexGrid.DecreaseVisibility(location, VisionRange);
               location.Unit = null;
            }
            location = value;
            value.Unit = this;
            hexGrid.IncreaseVisibility(location, VisionRange);
            transform.localPosition = value.Position;
         }
      }

      private void OnEnable() {
         if (location) {
            transform.localPosition = location.Position;
            if (currentTravelLocation) {
               hexGrid.IncreaseVisibility(location, VisionRange);
               hexGrid.DecreaseVisibility(currentTravelLocation, VisionRange);
               currentTravelLocation = null;
            }
         }
      }

      private IEnumerator TravelPath() {
         Vector3 a, b, c = pathToTravel[0].Position;
         yield return LookAt(pathToTravel[1].Position);
         hexGrid.DecreaseVisibility(
            currentTravelLocation ?? pathToTravel[0],
            VisionRange
         );

         float t = Time.deltaTime * travelSpeed;
         for (int i = 1; i < pathToTravel.Count; i++) {
            currentTravelLocation = pathToTravel[i];
            a = c;
            b = pathToTravel[i - 1].Position;
            c = (b + currentTravelLocation.Position) * 0.5f;
            hexGrid.IncreaseVisibility(pathToTravel[i], VisionRange);
            for (; t < 1f; t += Time.deltaTime * travelSpeed) {
               transform.localPosition = Bezier.GetPoint(a, b, c, t);
               Vector3 d = Bezier.GetDerivative(a, b, c, t);
               d.y = 0f;
               transform.localRotation = Quaternion.LookRotation(d);
               yield return null;
            }
            hexGrid.DecreaseVisibility(pathToTravel[i], VisionRange);
            t -= 1;
         }
         currentTravelLocation = null;

         a = c;
         b = location.Position;
         c = b;
         hexGrid.IncreaseVisibility(location, VisionRange);
         for (; t < 1f; t += Time.deltaTime * travelSpeed) {
            transform.localPosition = Bezier.GetPoint(a, b, c, t);
            Vector3 d = Bezier.GetDerivative(a, b, c, t);
            d.y = 0f;
            transform.localRotation = Quaternion.LookRotation(d);
            yield return null;
         }

         transform.localPosition = location.Position;
         orientation = transform.localRotation.eulerAngles.y;
         ListPool<HexCell>.Add(pathToTravel);
         pathToTravel = null;
      }

      private IEnumerator LookAt(Vector3 point) {
         point.y = transform.localPosition.y;
         Quaternion fromRotation = transform.localRotation;
         Quaternion toRotation = Quaternion.LookRotation(point - transform.localPosition);
         float angle = Quaternion.Angle(fromRotation, toRotation);

         if (angle > 0f) {
            float speed = rotationSpeed / angle;

            for (float t = Time.deltaTime * speed; t < 1f; t += Time.deltaTime * speed) {
               transform.localRotation =
                  Quaternion.Slerp(fromRotation, toRotation, t);
               yield return null;
            }
         }

         transform.LookAt(point);
         orientation = transform.localRotation.eulerAngles.y;
      }

      public void Initialize(HexGrid grid, Transform parent, HexCell location, float orientation) {
         hexGrid = grid;
         transform.SetParent(parent, false);
         Location = location;
         Orientation = orientation;
      }

      public void ValidateLocation() {
         transform.localPosition = location.Position;
      }

      public void Die() {
         hexGrid.DecreaseVisibility(location, VisionRange);
         location.Unit = null;
         Destroy(gameObject);
      }

      public void Save(BinaryWriter writer) {
         location.Coordinates.Save(writer);
         writer.Write(orientation);
      }

      public void Travel(List<HexCell> path) {
         location.Unit = null;
         location = path[path.Count - 1];
         location.Unit = this;
         pathToTravel = path;
         StopAllCoroutines();
         StartCoroutine(TravelPath());
      }

      public int GetMoveCost(HexCell fromCell, HexCell toCell, HexGridDirection direction) {
         HexEdgeType edgeType = fromCell.GetEdgeType(toCell);
         if (edgeType == HexEdgeType.Cliff) {
            return -1;
         }
         int moveCost;
         if (fromCell.HasRoadThroughEdge(direction)) {
            moveCost = 1;
         } else if (fromCell.Walled != toCell.Walled) {
            return -1;
         } else {
            moveCost = edgeType == HexEdgeType.Flat ? 5 : 10;
            moveCost +=
               toCell.UrbanLevel + toCell.FarmLevel + toCell.PlantLevel;
         }
         return moveCost;
      }

      /// <summary>
      /// Cells must be explored and not underwater or currently occupied by another unit to be a valid destination.
      /// </summary>
      /// <param name="cell"></param>
      /// <returns></returns>
      public bool IsValidDestination(HexCell cell) {
         return cell.IsExplored && !cell.IsUnderwater && !cell.Unit;
      }
   }
}