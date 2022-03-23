using System.Collections;
using System.Collections.Generic;
using HexMap.Map;
using UnityEngine;
using UnityEditor;

namespace HexMap.Editor
{
   /// <summary>
   /// This class will draw a custom view in the inspector for HexCoordinates mainly to handle the display of the coordinates.
   /// </summary>
   [CustomPropertyDrawer(typeof(HexCoordinates))]
   public class HexCoordinatesDrawer : PropertyDrawer
   {
      public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
      {
         // Draw Label
         GUI.Label(position, "Coordinates");
         position = EditorGUI.PrefixLabel(position, label);
         // Draw coordinates
         var coordinates =
            new HexCoordinates(
                  property.FindPropertyRelative("x").intValue,
                  property.FindPropertyRelative("z").intValue
            );
         GUI.Label(position, coordinates.ToString());
      }
   }
}
