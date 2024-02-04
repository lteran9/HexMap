using UnityEngine;
using UnityEditor;

namespace HexMap.EditorTools {
   /// <summary>
   /// Custom drawer for the ReadOnly attribute.
   /// </summary>
   [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
   public class ReadOnlyDrawer : PropertyDrawer {
#if UNITY_EDITOR
      public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
         bool previousGUIState = GUI.enabled;

         GUI.enabled = false;
         EditorGUI.PropertyField(position, property, label);
         GUI.enabled = previousGUIState;
      }
#endif
   }
}