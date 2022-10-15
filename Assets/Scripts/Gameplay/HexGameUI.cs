using HexMap.Map;
using HexMap.Input;
using HexMap.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Gameplay
{
   public class HexGameUI : MonoBehaviour
   {
      HexCell currentCell = default;
      HexUnit selectedUnit = default;

      [SerializeField] InputReader _inputReader = default;

      public HexGrid _grid = default;

      public void OnEnable()
      {
         _inputReader.MouseClick += DoSelection;
      }

      public void OnDisable()
      {
         _inputReader.MouseClick -= DoSelection;
      }

      bool UpdateCurrentCell()
      {
         Vector3 position = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
         Ray inputRay = Camera.main.ScreenPointToRay(position);
         HexCell cell = _grid.GetCell(inputRay);
         if (cell != currentCell)
         {
            currentCell = cell;
            return true;
         }
         return false;
      }

      void DoSelection()
      {
         UpdateCurrentCell();
         if (currentCell && currentCell.Unit)
         {
            selectedUnit = currentCell.Unit;
         }

         if (selectedUnit)
         {
            DoPathfinding();
         }
      }

      void DoPathfinding()
      {
         if (currentCell)
         {
            _grid.FindPath(selectedUnit.Location, currentCell, 24);
         }
         else
         {
            _grid.ClearPath();
         }
      }

      public void SetEditMode(bool toggle)
      {
         enabled = !toggle;
         _grid.ShowUI(!toggle);
      }
   }
}
