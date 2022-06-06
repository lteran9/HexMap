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
      public event UnityAction MouseDrag = delegate { };
      public event UnityAction MenuMouseClick = delegate { };
      public event UnityAction<float> ZoomCamera = delegate { };
      public event UnityAction<float> RotateEvent = delegate { };
      public event UnityAction<Vector2> MoveEvent = delegate { };

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

      public void OnMouseDrag(InputAction.CallbackContext context)
      {
         Debug.Log(context.ReadValue<Vector2>());
         if (context.phase == InputActionPhase.Performed)
            MouseDrag.Invoke();
      }

      public void OnZoom(InputAction.CallbackContext context)
      {
         if (context.phase == InputActionPhase.Performed)
            ZoomCamera.Invoke(context.ReadValue<float>());
      }

      public void OnCameraMove(InputAction.CallbackContext context)
      {
         MoveEvent.Invoke(context.ReadValue<Vector2>());
      }

      public void OnCameraRotate(InputAction.CallbackContext context)
      {
         RotateEvent.Invoke(context.ReadValue<float>());
      }
   }
}
