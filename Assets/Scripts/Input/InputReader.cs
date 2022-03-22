using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace HexMap.Input
{
   [CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
   public class InputReader : ScriptableObject, PlayerControls.IPlayerActions
   {
      public event UnityAction MenuMouseMove = delegate { };
      public event UnityAction MenuMouseClick = delegate { };
      public event UnityAction RotateCameraLeft = delegate { };
      public event UnityAction RotateCameraRight = delegate { };
      public event UnityAction<float> ZoomCamera = delegate { };

      private PlayerControls playerControls;

      private void OnEnable()
      {
         if (playerControls == null)
         {
            playerControls = new PlayerControls();
            playerControls.Player.SetCallbacks(this);
            playerControls.Player.Enable();
         }
      }

      private void OnDisable()
      {
         if (playerControls != null)
         {
            playerControls.Player.Disable();
         }
      }

      public void OnClick(InputAction.CallbackContext context)
      {
         if (context.phase == InputActionPhase.Performed)
         {
            MenuMouseClick.Invoke();
         }
      }

      public void OnMouseMove(InputAction.CallbackContext context)
      {
         if (context.phase == InputActionPhase.Performed)
            MenuMouseMove.Invoke();
      }

      public void OnRotateCameraLeft(InputAction.CallbackContext context)
      {
         if (context.phase == InputActionPhase.Performed)
            RotateCameraLeft.Invoke();
      }

      public void OnRotateCameraRight(InputAction.CallbackContext context)
      {
         if (context.phase == InputActionPhase.Performed)
            RotateCameraRight.Invoke();
      }

      public void OnZoom(InputAction.CallbackContext context)
      {
         if (context.phase == InputActionPhase.Performed)
         {
            ZoomCamera.Invoke(context.ReadValue<float>());
         }
      }
   }
}
