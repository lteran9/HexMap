using HexMap.Input;
using HexMap.Extensions;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HexMap.Map
{
   public class HexMapEditor : MonoBehaviour
   {
      [SerializeField] Color[] _colors;
      [SerializeField] HexGrid _hexGrid;
      [SerializeField] InputReader _inputReader = default;

      int activeElevation,
         activeWaterLevel,
         brushSize;

      bool applyColor,
         isDrag,
         applyElevation = true,
         applyWaterLevel = true;

      HexCell previousCell;
      Color activeColor;
      OptionalToggle riverMode = OptionalToggle.Ignore, roadMode = OptionalToggle.Ignore;
      HexGrid.HexDirection dragDirection;

      enum OptionalToggle
      {
         Ignore, Yes, No
      }

      void Awake()
      {
         SelectColor(0);
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
            if (applyColor && riverMode == OptionalToggle.Ignore && roadMode == OptionalToggle.Ignore)
            {
               cell.Color = activeColor;
            }
            if (applyElevation && riverMode == OptionalToggle.Ignore && roadMode == OptionalToggle.Ignore)
            {
               cell.Elevation = activeElevation;
            }
            if (applyWaterLevel && riverMode == OptionalToggle.Ignore && roadMode == OptionalToggle.Ignore)
            {
               cell.WaterLevel = activeWaterLevel;
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

      public void SelectColor(int index)
      {
         applyColor = index >= 0;
         if (applyColor)
         {
            activeColor = _colors[index];
         }
      }

      public void SetElevation(float elevation)
      {
         activeElevation = (int)elevation;
      }

      public void SetWaterLevel(float level)
      {
         activeWaterLevel = (int)level;
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

      public void SetRiverMode(int mode)
      {
         riverMode = (OptionalToggle)mode;
      }

      public void SetRoadMode(int mode)
      {
         roadMode = (OptionalToggle)mode;
      }

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