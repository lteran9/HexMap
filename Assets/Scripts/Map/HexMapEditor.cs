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

      int activeElevation;
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
            EditCell(hexGrid.GetCell(hit.point));
         }
      }

      void EditCell(HexCell cell)
      {
         cell.color = activeColor;
         cell.Elevation = activeElevation;
      }

      public void SelectColor(int index)
      {
         activeColor = colors[index];
      }

      public void SetElevation(float elevation)
      {
         activeElevation = (int)elevation;
      }

      public void ResetMap()
      {
         // TODO
      }
   }
}