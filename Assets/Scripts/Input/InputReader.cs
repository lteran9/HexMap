using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace HexMap.Input {
   [CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
   public class InputReader : ScriptableObject, PlayerControls.IGameplayActions {
      public event UnityAction MouseDrag = delegate { };
      public event UnityAction MouseClick = delegate { };
      public event UnityAction RightMouseClick = delegate { };
      public event UnityAction LeftShiftStarted = delegate { };
      public event UnityAction LeftShiftStopped = delegate { };
      public event UnityAction PlaceUnit = delegate { };
      public event UnityAction DestroyUnit = delegate { };
      public event UnityAction<float> ZoomCamera = delegate { };
      public event UnityAction<float> RotateEvent = delegate { };
      public event UnityAction<Vector2> MoveEvent = delegate { };

      private static PlayerControls playerControls;

      private void OnEnable() {
         if (playerControls == null) {
            playerControls = new PlayerControls();
            playerControls.Gameplay.SetCallbacks(this);
            playerControls.Gameplay.Enable();
         } else {
            EnableInput();
         }
      }

      private void OnDisable() {
         if (playerControls != null) {
            DisableInput();
         }
      }

      public void EnableInput() {
         playerControls.Gameplay.Enable();
      }

      public void DisableInput() {
         playerControls.Gameplay.Disable();
      }

      public void OnClick(InputAction.CallbackContext context) {
         if (context.phase == InputActionPhase.Performed) {
            MouseClick.Invoke();
         }
      }

      public void OnRightClick(InputAction.CallbackContext context) {
         if (context.phase == InputActionPhase.Performed) {
            RightMouseClick.Invoke();
         }
      }

      public void OnMouseDrag(InputAction.CallbackContext context) {
         if (context.phase == InputActionPhase.Performed)
            MouseDrag.Invoke();
      }

      public void OnZoom(InputAction.CallbackContext context) {
         if (context.phase == InputActionPhase.Performed)
            ZoomCamera.Invoke(context.ReadValue<float>());
      }

      public void OnCameraMove(InputAction.CallbackContext context) {
         Debug.Log(nameof(OnCameraMove));
         MoveEvent.Invoke(context.ReadValue<Vector2>());
      }

      public void OnCameraRotate(InputAction.CallbackContext context) {
         RotateEvent.Invoke(context.ReadValue<float>());
      }

      public void OnSearch(InputAction.CallbackContext context) {
         switch (context.phase) {
            case InputActionPhase.Performed:
               LeftShiftStarted.Invoke();
               break;
            case InputActionPhase.Canceled:
               LeftShiftStopped.Invoke();
               break;
            default:
               break;
         }
      }

      public void OnPlaceUnit(InputAction.CallbackContext context) {
         if (context.phase == InputActionPhase.Performed) {
            PlaceUnit.Invoke();
         }
      }
   }
}
