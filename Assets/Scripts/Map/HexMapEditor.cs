using System;
using UnityEngine;
using UnityEngine.EventSystems;
using HexMap.Input;

namespace HexMap.Map
{
   public class HexMapEditor : MonoBehaviour
   {
      [SerializeField] Color[] colors;
      [SerializeField] HexGrid hexGrid;
      [SerializeField] InputReader inputReader = default;

      int activeElevation, brushSize;
      bool applyColor, applyElevation = true;
      Color activeColor;

      void Awake()
      {
         SelectColor(0);
      }

      void Start()
      {
         inputReader.MenuMouseClick += OnClick;
      }

      void OnClick()
      {
         if (EventSystem.current.IsPointerOverGameObject() == false)
         {
            // Register click
            HandleInput();
         }
      }

      void HandleInput()
      {
         Vector3 position = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
         Ray inputRay = Camera.main.ScreenPointToRay(position);
         if (Physics.Raycast(inputRay, out RaycastHit hit))
         {
            EditCells(hexGrid.GetCell(hit.point));
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

      public void ResetMap()
      {
         // TODO
      }
   }
}