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
            onMouseClick.Invoke();
         }
      }
   }
}
