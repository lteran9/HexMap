using HexMap.Map;
using HexMap.Misc;
using System.Collections;
using System.Collections.Generic;
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

      const int visionRange = 2;

      const float travelSpeed = 4f;
      const float rotationSpeed = 180f;

      #endregion

      public int Speed
      {
         get
         {
            return 24;
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
               Grid.DecreaseVisibility(location, visionRange);
               location.Unit = null;
            }
            location = value;
            value.Unit = this;
            Grid.IncreaseVisibility(location, visionRange);
            transform.localPosition = value.Position;
         }
      }
      public HexGrid Grid { get; set; }

      float orientation = default;

      HexCell location = default,
         currentTravelLocation = null;

      List<HexCell> pathToTravel = default;

      void OnEnable()
      {
         if (location)
         {
            transform.localPosition = location.Position;
            if (currentTravelLocation)
            {
               Grid.IncreaseVisibility(location, visionRange);
               Grid.DecreaseVisibility(currentTravelLocation, visionRange);
               currentTravelLocation = null;
            }
         }
      }

      IEnumerator TravelPath()
      {
         Vector3 a, b, c = pathToTravel[0].Position;
         yield return LookAt(pathToTravel[1].Position);
         Grid.DecreaseVisibility(
            currentTravelLocation ?? pathToTravel[0],
            visionRange
         );

         float t = Time.deltaTime * travelSpeed;
         for (int i = 1; i < pathToTravel.Count; i++)
         {
            currentTravelLocation = pathToTravel[i];
            a = c;
            b = pathToTravel[i - 1].Position;
            c = (b + currentTravelLocation.Position) * 0.5f;
            Grid.IncreaseVisibility(pathToTravel[i], visionRange);
            for (; t < 1f; t += Time.deltaTime * travelSpeed)
            {
               transform.localPosition = Bezier.GetPoint(a, b, c, t);
               Vector3 d = Bezier.GetDerivative(a, b, c, t);
               d.y = 0f;
               transform.localRotation = Quaternion.LookRotation(d);
               yield return null;
            }
            Grid.DecreaseVisibility(pathToTravel[i], visionRange);
            t -= 1;
         }
         currentTravelLocation = null;

         a = c;
         b = location.Position;
         c = b;
         Grid.IncreaseVisibility(location, visionRange);
         for (; t < 1f; t += Time.deltaTime * travelSpeed)
         {
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

      IEnumerator LookAt(Vector3 point)
      {
         point.y = transform.localPosition.y;
         Quaternion fromRotation = transform.localRotation;
         Quaternion toRotation = Quaternion.LookRotation(point - transform.localPosition);
         float angle = Quaternion.Angle(fromRotation, toRotation);

         if (angle > 0f)
         {
            float speed = rotationSpeed / angle;

            for (float t = Time.deltaTime * speed; t < 1f; t += Time.deltaTime * speed)
            {
               transform.localRotation =
                  Quaternion.Slerp(fromRotation, toRotation, t);
               yield return null;
            }
         }

         transform.LookAt(point);
         orientation = transform.localRotation.eulerAngles.y;
      }

      public void ValidateLocation()
      {
         transform.localPosition = location.Position;
      }

      public void Die()
      {
         if (location)
         {
            Grid.DecreaseVisibility(location, visionRange);
         }
         location.Unit = null;
         Destroy(gameObject);
      }

      public void Save(BinaryWriter writer)
      {
         location.Coordinates.Save(writer);
         writer.Write(orientation);
      }

      public void Travel(List<HexCell> path)
      {
         location.Unit = null;
         location = path[path.Count - 1];
         location.Unit = this;
         pathToTravel = path;
         StopAllCoroutines();
         StartCoroutine(TravelPath());
      }

      public int GetMoveCost(HexCell fromCell, HexCell toCell, HexGrid.HexDirection direction)
      {
         HexGrid.HexEdgeType edgeType = fromCell.GetEdgeType(toCell);
         if (edgeType == HexGrid.HexEdgeType.Cliff)
         {
            return -1;
         }
         int moveCost;
         if (fromCell.HasRoadThroughEdge(direction))
         {
            moveCost = 1;
         }
         else if (fromCell.Walled != toCell.Walled)
         {
            return -1;
         }
         else
         {
            moveCost = edgeType == HexGrid.HexEdgeType.Flat ? 5 : 10;
            moveCost +=
               toCell.UrbanLevel + toCell.FarmLevel + toCell.PlantLevel;
         }
         return moveCost;
      }

      public bool IsValidDestination(HexCell cell)
      {
         return cell.IsExplored && !cell.IsUnderwater && !cell.Unit;
      }
   }
}