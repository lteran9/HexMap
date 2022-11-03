using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;

namespace HexMap.UI
{
   public class UIHandler
   {
      public event UnityAction<int> FeatureToggleChanged = delegate { };
      public event UnityAction<int> ElevationSlider = delegate { };
      public event UnityAction<bool> ElevationToggle = delegate { };
      public event UnityAction<int> WaterSlider = delegate { };
      public event UnityAction<bool> WaterToggle = delegate { };

      public event UnityAction<int> RiverModeChanged = delegate { };
      public event UnityAction<int> RoadModeChanged = delegate { };

      public event UnityAction<int> BrushSizeSlider = delegate { };

      #region Checkbox

      public Toggle _none = default;
      public Toggle _sand = default;
      public Toggle _grass = default;
      public Toggle _mud = default;
      public Toggle _stone = default;
      public Toggle _snow = default;

      public Toggle _elevationApply = default;
      public Toggle _waterApply = default;

      public Toggle _riverNone = default;
      public Toggle _riverYes = default;
      public Toggle _riverNo = default;

      public Toggle _roadNone = default;
      public Toggle _roadYes = default;
      public Toggle _roadNo = default;

      #endregion

      #region Slider

      SliderInt _elevationLevel = default;
      SliderInt _waterLevel = default;

      #endregion 

      public UIHandler(VisualElement rootVisualElement)
      {
         if (rootVisualElement != null)
         {
            #region LeftSideMenu

            #region Register callbacks for FeatureToggle

            _none = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_None));
            if (_none != null)
            {
               _none.RegisterValueChangedCallback(ToggleNoneValue);
            }
            _sand = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_Sand));
            if (_sand != null)
            {
               _sand.RegisterValueChangedCallback(ToggleSandValue);
            }
            _grass = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_Grass));
            if (_grass != null)
            {
               _grass.RegisterValueChangedCallback(ToggleGrassValue);
            }
            _mud = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_Mud));
            if (_mud != null)
            {
               _mud.RegisterValueChangedCallback(ToggleMudValue);
            }
            _stone = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_Stone));
            if (_stone != null)
            {
               _stone.RegisterValueChangedCallback(ToggleStoneValue);
            }
            _snow = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_Snow));
            if (_snow != null)
            {
               _snow.RegisterValueChangedCallback(ToggleSnowValue);
            }

            #endregion

            #region Register callbacks for Elevation

            _elevationApply = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_Elevation));
            if (_elevationApply != null)
            {
               _elevationApply.RegisterValueChangedCallback(ToggleElevationValue);
            }
            _elevationLevel = rootVisualElement.Q<SliderInt>(nameof(UIDocumentNames.SliderInt_Elevation));
            if (_elevationLevel != null)
            {
               _elevationLevel.RegisterValueChangedCallback(SliderIntElevationValue);
            }

            #endregion

            #region Register callbacks for Water

            _waterApply = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_Water));
            if (_waterApply != null)
            {
               _waterApply.RegisterValueChangedCallback(ToggleWaterValue);
            }
            _waterLevel = rootVisualElement.Q<SliderInt>(nameof(UIDocumentNames.SliderInt_Water));
            if (_waterLevel != null)
            {
               _waterLevel.RegisterValueChangedCallback(SliderIntWaterValue);
            }

            #endregion

            #region Register callbacks for River

            _riverNone = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_RiverNone));
            if (_riverNone != null)
            {
               _riverNone.RegisterValueChangedCallback(ToggleRiverNoneValue);
            }
            _riverYes = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_RiverYes));
            if (_riverYes != null)
            {
               _riverYes.RegisterValueChangedCallback(ToggleRiverYesValue);
            }
            _riverNo = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_RiverNo));
            if (_riverNo != null)
            {
               _riverNo.RegisterValueChangedCallback(ToggleRiverNoValue);
            }

            #endregion

            #region Register callbacks for Road

            _roadNone = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_RoadNone));
            if (_roadNone != null)
            {
               _roadNone.RegisterValueChangedCallback(ToggleRoadNoneValue);
            }
            _roadYes = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_RoadYes));
            if (_roadYes != null)
            {
               _roadYes.RegisterValueChangedCallback(ToggleRoadYesValue);
            }
            _roadNo = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_RoadNo));
            if (_roadNo != null)
            {
               _roadNo.RegisterValueChangedCallback(ToggleRoadNoValue);
            }

            #endregion

            #region Register callbacks for Brush Size

            rootVisualElement.Q<SliderInt>(nameof(UIDocumentNames.SliderInt_BrushSize))
               .RegisterValueChangedCallback(evt =>
               {
                  BrushSizeSlider.Invoke(evt.newValue);
               });

            #endregion

            #endregion

            #region RightSideMenu



            #endregion
         }
      }

      #region Feature Toggles

      void ToggleNoneValue(ChangeEvent<bool> evt)
      {
         if (evt.newValue)
         {
            FeatureToggleChanged.Invoke(-1);

            _sand.SetValueWithoutNotify(false);
            _grass.SetValueWithoutNotify(false);
            _mud.SetValueWithoutNotify(false);
            _stone.SetValueWithoutNotify(false);
            _snow.SetValueWithoutNotify(false);
         }
      }

      void ToggleSandValue(ChangeEvent<bool> evt)
      {
         if (evt.newValue)
         {
            FeatureToggleChanged.Invoke(0);

            _none.SetValueWithoutNotify(false);
            _grass.SetValueWithoutNotify(false);
            _mud.SetValueWithoutNotify(false);
            _stone.SetValueWithoutNotify(false);
            _snow.SetValueWithoutNotify(false);
         }
         else
         {
            FeatureToggleChanged.Invoke(-1);
         }
      }

      void ToggleGrassValue(ChangeEvent<bool> evt)
      {
         if (evt.newValue)
         {
            FeatureToggleChanged.Invoke(1);

            _none.SetValueWithoutNotify(false);
            _sand.SetValueWithoutNotify(false);
            _mud.SetValueWithoutNotify(false);
            _stone.SetValueWithoutNotify(false);
            _snow.SetValueWithoutNotify(false);
         }
         else
         {
            FeatureToggleChanged.Invoke(-1);
         }
      }

      void ToggleMudValue(ChangeEvent<bool> evt)
      {
         if (evt.newValue)
         {
            FeatureToggleChanged.Invoke(2);

            _none.SetValueWithoutNotify(false);
            _sand.SetValueWithoutNotify(false);
            _grass.SetValueWithoutNotify(false);
            _stone.SetValueWithoutNotify(false);
            _snow.SetValueWithoutNotify(false);
         }
         else
         {
            FeatureToggleChanged.Invoke(-1);
         }
      }

      void ToggleStoneValue(ChangeEvent<bool> evt)
      {
         if (evt.newValue)
         {
            FeatureToggleChanged.Invoke(3);

            _none.SetValueWithoutNotify(false);
            _sand.SetValueWithoutNotify(false);
            _grass.SetValueWithoutNotify(false);
            _mud.SetValueWithoutNotify(false);
            _snow.SetValueWithoutNotify(false);
         }
         else
         {
            FeatureToggleChanged.Invoke(-1);
         }
      }

      void ToggleSnowValue(ChangeEvent<bool> evt)
      {
         if (evt.newValue)
         {
            FeatureToggleChanged.Invoke(4);

            _none.SetValueWithoutNotify(false);
            _sand.SetValueWithoutNotify(false);
            _grass.SetValueWithoutNotify(false);
            _mud.SetValueWithoutNotify(false);
            _stone.SetValueWithoutNotify(false);
         }
         else
         {
            FeatureToggleChanged.Invoke(-1);
         }
      }

      #endregion

      #region Elevation

      void ToggleElevationValue(ChangeEvent<bool> evt)
      {
         ElevationToggle.Invoke(evt.newValue);
      }

      void SliderIntElevationValue(ChangeEvent<int> evt)
      {
         ElevationSlider.Invoke(evt.newValue);
      }

      #endregion

      #region Water

      void ToggleWaterValue(ChangeEvent<bool> evt)
      {
         WaterToggle.Invoke(evt.newValue);
      }

      void SliderIntWaterValue(ChangeEvent<int> evt)
      {
         WaterSlider.Invoke(evt.newValue);
      }

      #endregion

      #region River 

      void ToggleRiverNoneValue(ChangeEvent<bool> evt)
      {
         if (evt.newValue)
         {
            RiverModeChanged.Invoke(0);
            _riverNo.SetValueWithoutNotify(false);
            _riverYes.SetValueWithoutNotify(false);
         }
      }

      void ToggleRiverYesValue(ChangeEvent<bool> evt)
      {
         if (evt.newValue)
         {
            RiverModeChanged.Invoke(1);
            _riverNo.SetValueWithoutNotify(false);
            _riverNone.SetValueWithoutNotify(false);
         }
         else
         {
            RiverModeChanged.Invoke(0);
         }
      }

      void ToggleRiverNoValue(ChangeEvent<bool> evt)
      {
         if (evt.newValue)
         {
            RiverModeChanged.Invoke(2);
            _riverNone.SetValueWithoutNotify(false);
            _riverYes.SetValueWithoutNotify(false);
         }
         else
         {
            RiverModeChanged.Invoke(0);
         }
      }

      #endregion 

      #region Road 

      void ToggleRoadNoneValue(ChangeEvent<bool> evt)
      {
         if (evt.newValue)
         {
            RoadModeChanged.Invoke(0);
            _roadNo.SetValueWithoutNotify(false);
            _roadYes.SetValueWithoutNotify(false);
         }
      }

      void ToggleRoadYesValue(ChangeEvent<bool> evt)
      {
         if (evt.newValue)
         {
            RoadModeChanged.Invoke(1);
            _roadNo.SetValueWithoutNotify(false);
            _roadNone.SetValueWithoutNotify(false);
         }
         else
         {
            RoadModeChanged.Invoke(0);
         }
      }

      void ToggleRoadNoValue(ChangeEvent<bool> evt)
      {
         if (evt.newValue)
         {
            RoadModeChanged.Invoke(2);
            _roadNone.SetValueWithoutNotify(false);
            _roadYes.SetValueWithoutNotify(false);
         }
         else
         {
            RoadModeChanged.Invoke(0);
         }
      }

      #endregion 

   }
}