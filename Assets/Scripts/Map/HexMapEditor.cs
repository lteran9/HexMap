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

      Color activeColor;

      void Awake()
      {
         SelectColor(0);
      }

      void Start()
      {
         inputReader.onMouseClick += OnClick;
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
            hexGrid.ColorCell(hit.point, activeColor);
         }
      }

      public void SelectColor(int index)
      {
         activeColor = colors[index];
      }
   }
}