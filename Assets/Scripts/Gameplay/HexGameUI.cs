using HexMap.Map;
using HexMap.Input;
using HexMap.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Gameplay {
   public class HexGameUI : MonoBehaviour {
      [SerializeField] private HexGrid _grid = default;
      [SerializeField] private InputReader _inputReader = default;

      private HexCell currentCell = default;
      private HexUnit selectedUnit = default;

      private void Start() {
         SetEditMode(true);
      }

      private void OnEnable() {
         _inputReader.MouseClick += DoSelection;
         _inputReader.RightMouseClick += DoMove;
      }

      private void OnDisable() {
         _inputReader.MouseClick -= DoSelection;
         _inputReader.RightMouseClick -= DoMove;
      }

      private bool UpdateCurrentCell() {
         Vector3 position = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
         Ray inputRay = Camera.main.ScreenPointToRay(position);
         HexCell cell = _grid.GetCell(inputRay);
         if (cell != currentCell) {
            currentCell = cell;
            return true;
         }
         return false;
      }

      private void DoMove() {
         if (selectedUnit) {
            if (_grid.HasPath) {
               selectedUnit.Travel(_grid.GetPath());
               _grid.ClearPath();
            }
         }
      }

      private void DoSelection() {
         _grid.ClearPath();
         UpdateCurrentCell();
         if (currentCell && currentCell.Unit) {
            if (selectedUnit) {
               selectedUnit.Location.DisableHighlight();
            }
            currentCell.EnableHighlight(Color.blue);
            selectedUnit = currentCell.Unit;
         }

         if (selectedUnit) {
            DoPathfinding();
         }
      }

      private void DoPathfinding() {
         if (currentCell && selectedUnit.IsValidDestination(currentCell)) {
            _grid.FindPath(selectedUnit.Location, currentCell, selectedUnit);
         } else {
            _grid.ClearPath();
         }
      }

      public void SetEditMode(bool toggle) {
         enabled = !toggle;
         _grid.ShowUI(!toggle);
         _grid.ClearPath();
         if (toggle) {
            Shader.EnableKeyword("_HEX_MAP_EDIT_MODE_ON");
         } else {
            Shader.DisableKeyword("_HEX_MAP_EDIT_MODE_ON");
         }
      }
   }
}
