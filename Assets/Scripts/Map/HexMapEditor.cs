using HexMap.Input;
using HexMap.Extensions;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HexMap.Map
{
   public class HexMapEditor : MonoBehaviour
   {
      [SerializeField] Color[] colors;
      [SerializeField] HexGrid hexGrid;
      [SerializeField] InputReader inputReader = default;

      int activeElevation, brushSize;
      bool applyColor, isDrag, applyElevation = true;
      HexGrid.HexDirection dragDirection;
      HexCell previousCell;
      Color activeColor;
      OptionalToggle riverMode;

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
         inputReader.MenuMouseClick += OnClick;
      }

      void OnDisable()
      {
         inputReader.MenuMouseClick -= OnClick;
      }

      void OnClick()
      {
         if (EventSystem.current.IsPointerOverGameObject() == false)
         {
            // Register click
            HandleInput();
         }
         else
         {
            previousCell = null;
         }
      }

      void HandleInput()
      {
         Vector3 position = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
         Ray inputRay = Camera.main.ScreenPointToRay(position);
         if (Physics.Raycast(inputRay, out RaycastHit hit))
         {
            HexCell currentCell = hexGrid.GetCell(hit.point);
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
            if (applyColor)
            {
               cell.Color = activeColor;
            }
            if (applyElevation)
            {
               cell.Elevation = activeElevation;
            }

            if (riverMode == OptionalToggle.No)
            {
               cell.RemoveRiver();
            }
            else if (isDrag && riverMode == OptionalToggle.Yes)
            {
               HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
               if (otherCell)
               {
                  otherCell.SetOutgoingRiver(dragDirection);
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
               EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
         }

         for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
         {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
               EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
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
            activeColor = colors[index];
         }
      }

      public void SetElevation(float elevation)
      {
         activeElevation = (int)elevation;
      }

      public void SetBrushSize(float size)
      {
         brushSize = (int)size;
      }

      public void SetApplyElevation(bool toggle)
      {
         applyElevation = toggle;
      }

      public void SetRiverMode(int mode)
      {
         riverMode = (OptionalToggle)mode;
      }

      public void ShowUI(bool visible)
      {
         hexGrid.ShowUI(visible);
      }

      public void ResetMap()
      {
         // TODO
      }
   }
}