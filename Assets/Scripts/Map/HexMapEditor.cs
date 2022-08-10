using HexMap.Input;
using HexMap.Extensions;
using HexMap.Gameplay;
using System;
using System.IO;
using UnityEngine;

namespace HexMap.Map
{
   public class HexMapEditor : MonoBehaviour
   {
      [SerializeField] HexGrid _hexGrid = default;
      [SerializeField] InputReader _inputReader = default;

      int activeElevation,
         activeWaterLevel,
         activeUrbanLevel,
         activeFarmLevel,
         activePlantLevel,
         activeSpecialIndex,
         activeTerrainTypeIndex,
         brushSize;

      bool isDrag,
         applyElevation = true,
         applyWaterLevel = false,
         applyUrbanLevel = false,
         applyFarmLevel = false,
         applyPlantLevel = false,
         applySpecialIndex = false;

      HexCell previousCell;
      OptionalToggle riverMode = OptionalToggle.Ignore,
         roadMode = OptionalToggle.Ignore,
         walledMode = OptionalToggle.Ignore;
      HexGrid.HexDirection dragDirection;

      enum OptionalToggle
      {
         Ignore, Yes, No
      }

      void Awake()
      {
         //
      }

      void OnEnable()
      {
         _inputReader.MenuMouseClick += OnClick;
         _inputReader.MouseDrag += OnClick;
      }

      void OnDisable()
      {
         _inputReader.MenuMouseClick -= OnClick;
         _inputReader.MouseDrag -= OnClick;
      }

      void OnClick()
      {
         // if (EventSystem.current.IsPointerOverGameObject() == false)
         // {
         HandleInput();
         // }
         // else
         // {
         //    previousCell = null;
         // }
      }

      void HandleInput()
      {
         Vector3 position = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
         Ray inputRay = Camera.main.ScreenPointToRay(position);
         if (Physics.Raycast(inputRay, out RaycastHit hit))
         {
            HexCell currentCell = _hexGrid.GetCell(hit.point);
            if (previousCell && previousCell != currentCell)
            {
               ValidateDrag(currentCell);
            }
            else
            {
               isDrag = false;
            }
            EditCells(currentCell);
            previousCell = currentCell;
         }
         else
         {
            previousCell = null;
         }
      }

      void EditCell(HexCell cell)
      {
         if (cell != null)
         {
            if (activeTerrainTypeIndex >= 0)
            {
               cell.TerrainTypeIndex = activeTerrainTypeIndex;
            }
            if (applyElevation && riverMode == OptionalToggle.Ignore && roadMode == OptionalToggle.Ignore)
            {
               cell.Elevation = activeElevation;
            }
            if (applyWaterLevel && riverMode == OptionalToggle.Ignore && roadMode == OptionalToggle.Ignore)
            {
               cell.WaterLevel = activeWaterLevel;
            }
            if (applyUrbanLevel)
            {
               cell.UrbanLevel = activeUrbanLevel;
            }
            if (applyFarmLevel)
            {
               cell.FarmLevel = activeFarmLevel;
            }
            if (applyPlantLevel)
            {
               cell.PlantLevel = activePlantLevel;
            }
            if (applySpecialIndex)
            {
               cell.SpecialIndex = activeSpecialIndex;
            }

            if (riverMode == OptionalToggle.No)
            {
               var tempBrushSize = brushSize;
               brushSize = 0;
               cell.RemoveRiver();
               brushSize = tempBrushSize;
            }

            if (roadMode == OptionalToggle.No)
            {
               var tempBrushSize = brushSize;
               brushSize = 0;
               cell.RemoveRoads();
               brushSize = tempBrushSize;
            }

            if (walledMode != OptionalToggle.Ignore)
            {
               cell.Walled = walledMode == OptionalToggle.Yes;
            }

            if (isDrag)
            {
               HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
               if (otherCell)
               {
                  if (riverMode == OptionalToggle.Yes)
                  {
                     otherCell.SetOutgoingRiver(dragDirection);
                  }

                  if (roadMode == OptionalToggle.Yes)
                  {
                     otherCell.AddRoad(dragDirection);
                  }
               }
            }
         }
      }

      void EditCells(HexCell center)
      {
         int centerX = center.coordinates.X;
         int centerZ = center.coordinates.Z;

         for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
         {
            for (int x = centerX - r; x <= centerX + brushSize; x++)
            {
               EditCell(_hexGrid.GetCell(new HexCoordinates(x, z)));
            }
         }

         for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
         {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
               EditCell(_hexGrid.GetCell(new HexCoordinates(x, z)));
            }
         }
      }

      #region UI

      public void SetElevation(float elevation)
      {
         activeElevation = (int)elevation;
      }

      public void SetWaterLevel(float level)
      {
         activeWaterLevel = (int)level;
      }

      public void SetUrbanLevel(float level)
      {
         activeUrbanLevel = (int)level;
      }

      public void SetFarmLevel(float level)
      {
         activeFarmLevel = (int)level;
      }

      public void SetPlantLevel(float level)
      {
         activePlantLevel = (int)level;
      }

      public void SetSpecialIndex(float index)
      {
         activeSpecialIndex = (int)index;
      }

      public void SetBrushSize(float size)
      {
         brushSize = (int)size;
      }

      public void SetApplyElevation(bool toggle)
      {
         applyElevation = toggle;
      }

      public void SetApplyWaterLevel(bool toggle)
      {
         applyWaterLevel = toggle;
      }

      public void SetApplyUrbanLevel(bool toggle)
      {
         applyUrbanLevel = toggle;
      }

      public void SetApplyFarmLevel(bool toggle)
      {
         applyFarmLevel = toggle;
      }

      public void SetApplyPlantLevel(bool toggle)
      {
         applyPlantLevel = toggle;
      }

      public void SetApplySpecialIndex(bool toggle)
      {
         applySpecialIndex = toggle;
      }

      public void SetRiverMode(int mode)
      {
         riverMode = (OptionalToggle)mode;
      }

      public void SetRoadMode(int mode)
      {
         roadMode = (OptionalToggle)mode;
      }

      public void SetWalledMode(int mode)
      {
         walledMode = (OptionalToggle)mode;
      }

      public void SetTerrainTypeIndex(int index)
      {
         activeTerrainTypeIndex = index;
      }

      void ValidateDrag(HexCell currentCell)
      {
         for (dragDirection = HexGrid.HexDirection.NE; dragDirection <= HexGrid.HexDirection.NW; dragDirection++)
         {
            if (previousCell.GetNeighbor(dragDirection) == currentCell)
            {
               isDrag = true;
               return;
            }
         }
         isDrag = false;
      }

      #endregion

      #region Data Storage

      public void Save()
      {
         string path = Path.Combine(Application.persistentDataPath, "test.map");
         using (var writer = new BinaryWriter(File.Open(path, FileMode.Create)))
         {
            writer.Write(0);
            _hexGrid.Save(writer);
         }

      }

      public void Load()
      {
         string path = Path.Combine(Application.persistentDataPath, "test.map");
         using (var reader = new BinaryReader(File.OpenRead(path)))
         {
            int header = reader.ReadInt32();
            if (header <= 1)
            {
               _hexGrid.Load(reader, header);
               CameraManager.ValidatePosition();
            }
            else
            {
               Debug.LogWarning("Unknown map format " + header);
            }
         }
      }

      #endregion

      public void ShowUI(bool visible)
      {
         _hexGrid.ShowUI(visible);
      }

      public void ResetMap()
      {
         // TODO
      }
   }
}