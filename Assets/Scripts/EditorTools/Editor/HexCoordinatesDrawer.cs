using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HexMap.Map.Grid;
using System;

namespace HexMap.Editor {
   /// <summary>
   /// This class will draw a custom view in the inspector for HexCoordinates mainly to handle the display of the coordinates.
   /// </summary>
   [CustomPropertyDrawer(typeof(HexCoordinates))]
   [Obsolete]
   public class HexCoordinatesDrawer : PropertyDrawer {
      public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
         // Draw Label
         GUI.Label(position, "Coordinates");
         // Update position of element
         position = EditorGUI.PrefixLabel(position, label);
         // Create coordinates struct
         var coordinates =
            new HexCoordinates(
               property.FindPropertyRelative("x").intValue,
               property.FindPropertyRelative("z").intValue
            );
         position.x = Screen.width - 100;
         // Draw coordinates
         GUI.Label(position, coordinates.ToString());
      }
   }
}
