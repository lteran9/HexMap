using HexMap.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Gameplay
{
   public class NewMapMenu : MonoBehaviour
   {
      bool generateMaps = true;

      [SerializeField] HexGrid _hexGrid = default;
      [SerializeField] HexMapGenerator _mapGenerator = default;

      void CreateMap(int x, int z)
      {
         if (generateMaps)
         {
            _mapGenerator.GenerateMap(x, z);
         }
         else
         {
            _hexGrid.CreateMap(x, z);
         }
         CameraManager.ValidatePosition();
         Close();
      }

      public void CreateSmallMap()
      {
         CreateMap(20, 15);
      }

      public void CreateMediumMap()
      {
         CreateMap(40, 30);
      }

      public void CreateLargeMap()
      {
         CreateMap(80, 60);
      }

      public void Open()
      {
         gameObject.SetActive(true);
         CameraManager.Locked = true;
      }

      public void Close()
      {
         gameObject.SetActive(false);
         CameraManager.Locked = false;
      }

      public void ToggleMapGeneration(bool toggle)
      {
         generateMaps = toggle;
      }
   }
}