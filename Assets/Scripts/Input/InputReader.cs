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
      public event UnityAction onMouseClick = delegate { };
      public event UnityAction<Vector2> onMove = delegate { };

      private PlayerControls playerControls;

      private void OnEnable()
      {
         if (playerControls == null)
         {
            playerControls = new PlayerControls();
            playerControls.Player.SetCallbacks(this);
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
            Debug.Log("Click Invoked");
            onMouseClick.Invoke();
         }
      }

      public void OnMove(InputAction.CallbackContext context)
      {
         Debug.Log("Move Invoked");
         var direction = context.ReadValue<Vector2>();
         onMove.Invoke(direction);
      }
   }
}
