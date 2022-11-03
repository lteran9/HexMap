using HexMap.Input;
using HexMap.Extensions;
using HexMap.UI;
using HexMap.Units;
using UnityEngine;
using UnityEngine.UIElements;

namespace HexMap.Map
{
   public class HexMapEditor : MonoBehaviour
   {
      [SerializeField] HexGrid _hexGrid = default;
      [SerializeField] InputReader _inputReader = default;
      [SerializeField] Material _terrainMaterial = default;

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
         applySpecialIndex = false,
         leftShiftActive;

      UIHandler uiHandler = default;
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
         ShowGrid(false);
         Shader.EnableKeyword("_HEX_MAP_EDIT_MODE");
         SetEditMode(false);

         var uiDocument = GetComponentInChildren<UIDocument>();
         if (uiDocument)
         {
            var root = uiDocument.rootVisualElement;
            uiHandler = new UIHandler(root);
            uiHandler.FeatureToggleChanged += SetTerrainTypeIndex;

            uiHandler.ElevationSlider += SetElevation;
            uiHandler.ElevationToggle += SetApplyElevation;
            uiHandler.WaterSlider += SetWaterLevel;
            uiHandler.WaterToggle += SetApplyWaterLevel;
            uiHandler.RiverModeChanged += SetRiverMode;
            uiHandler.RoadModeChanged += SetRoadMode;
            uiHandler.BrushSizeSlider += SetBrushSize;
         }

      }

      void OnEnable()
      {
         _inputReader.MouseClick += OnClick;
         _inputReader.MouseDrag += OnClick;
         _inputReader.LeftShiftStarted += LeftShiftBeingHeld;
         _inputReader.LeftShiftStopped += LeftShiftReleased;
         _inputReader.PlaceUnit += HandleUnitInput;
      }

      void OnDisable()
      {
         _inputReader.MouseClick -= OnClick;
         _inputReader.MouseDrag -= OnClick;
         _inputReader.LeftShiftStarted -= LeftShiftBeingHeld;
         _inputReader.LeftShiftStopped -= LeftShiftReleased;
         _inputReader.PlaceUnit -= HandleUnitInput;
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
         HexCell currentCell = GetCellUnderCursor();
         if (currentCell != null)
         {
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
         int centerX = center.Coordinates.X;
         int centerZ = center.Coordinates.Z;

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

      public void SetElevation(int elevation)
      {
         activeElevation = elevation;
      }

      public void SetElevation(float elevation)
      {
         activeElevation = (int)elevation;
      }

      public void SetWaterLevel(int level)
      {
         activeWaterLevel = level;
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

      public void SetBrushSize(int size)
      {
         brushSize = size;
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

      public void SetEditMode(bool toggle)
      {
         enabled = toggle;
      }

      public void ShowGrid(bool visible)
      {
         if (_terrainMaterial != null)
         {
            _terrainMaterial.SetFloat("_GridOn", visible ? 1f : 0f);
         }
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

      void LeftShiftBeingHeld()
      {
         leftShiftActive = true;
      }

      void LeftShiftReleased()
      {
         leftShiftActive = false;
      }

      void HandleUnitInput()
      {
         if (leftShiftActive)
         {
            DestroyUnit();
         }
         else
         {
            CreateUnit();
         }
      }

      void CreateUnit()
      {
         HexCell cell = GetCellUnderCursor();
         if (cell)
         {
            if (!cell.Unit)
            {
               _hexGrid.AddUnit(
                  Instantiate(HexUnit.unitPrefab), cell, Random.Range(0f, 360f)
               );
            }
            else
            {
               Debug.Log("Unit already placed at location: " + cell.Coordinates.ToString());
            }
         }
      }

      void DestroyUnit()
      {
         HexCell cell = GetCellUnderCursor();
         if (cell && cell.Unit)
         {
            _hexGrid.RemoveUnit(cell.Unit);
         }
      }

      HexCell GetCellUnderCursor()
      {
         Vector3 position = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
         Ray inputRay = Camera.main.ScreenPointToRay(position);
         return _hexGrid.GetCell(inputRay);
      }
   }
}