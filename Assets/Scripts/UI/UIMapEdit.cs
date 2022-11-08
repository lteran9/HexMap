using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;

namespace HexMap.UI
{
   [RequireComponent(typeof(UIDocument))]
   public class UIMapEdit : MonoBehaviour
   {
      // Root of all functionality
      UIDocument _uiDocument = default;

      #region Actions

      public event UnityAction SaveEvent = delegate { };
      public event UnityAction LoadEvent = delegate { };
      public event UnityAction NewGameEvent = delegate { };

      public event UnityAction<int> FeatureToggleChanged = delegate { };
      public event UnityAction<int> ElevationSlider = delegate { };
      public event UnityAction<bool> ElevationToggle = delegate { };
      public event UnityAction<int> WaterSlider = delegate { };
      public event UnityAction<bool> WaterToggle = delegate { };

      public event UnityAction<int> RiverModeChanged = delegate { };
      public event UnityAction<int> RoadModeChanged = delegate { };
      public event UnityAction<int> WallModeChanged = delegate { };

      public event UnityAction<int> BrushSizeSlider = delegate { };
      public event UnityAction<int> UrbanSlider = delegate { };
      public event UnityAction<int> FarmSlider = delegate { };
      public event UnityAction<int> PlantSlider = delegate { };
      public event UnityAction<int> SpecialSlider = delegate { };

      public event UnityAction<bool> UrbanToggle = delegate { };
      public event UnityAction<bool> FarmToggle = delegate { };
      public event UnityAction<bool> PlantToggle = delegate { };
      public event UnityAction<bool> SpecialToggle = delegate { };
      public event UnityAction<bool> EditModeToggle = delegate { };

      #endregion

      #region Checkbox

      Toggle _none = default;
      Toggle _sand = default;
      Toggle _grass = default;
      Toggle _mud = default;
      Toggle _stone = default;
      Toggle _snow = default;

      Toggle _elevationApply = default;
      Toggle _waterApply = default;

      Toggle _riverNone = default;
      Toggle _riverYes = default;
      Toggle _riverNo = default;

      Toggle _roadNone = default;
      Toggle _roadYes = default;
      Toggle _roadNo = default;

      Toggle _urban = default;
      Toggle _farm = default;
      Toggle _plant = default;
      Toggle _special = default;

      Toggle _wallNone = default;
      Toggle _wallYes = default;
      Toggle _wallNo = default;

      Toggle _editMode = default;

      #endregion

      #region Slider

      SliderInt _elevationLevel = default;
      SliderInt _waterLevel = default;
      SliderInt _urbanLevel = default;
      SliderInt _farmLevel = default;
      SliderInt _plantLevel = default;
      SliderInt _specialLevel = default;

      #endregion

      void Awake()
      {
         _uiDocument = GetComponent<UIDocument>();
      }

      void OnEnable()
      {
         VisualElement rootVisualElement = _uiDocument?.rootVisualElement;

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

            #region Features 

            _urban = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_Urban));
            if (_urban != null)
            {
               _urban.RegisterValueChangedCallback(ToggleUrbanValue);
            }
            _farm = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_Farm));
            if (_farm != null)
            {
               _farm.RegisterValueChangedCallback(ToggleFarmValue);
            }
            _plant = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_Plant));
            if (_plant != null)
            {
               _plant.RegisterValueChangedCallback(TogglePlantValue);
            }
            _special = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_Special));
            if (_special != null)
            {
               _special.RegisterValueChangedCallback(ToggleSpecialValue);
            }

            _urbanLevel = rootVisualElement.Q<SliderInt>(nameof(UIDocumentNames.SliderInt_Urban));
            if (_urbanLevel != null)
            {
               _urbanLevel.RegisterValueChangedCallback(SliderIntUrbanValue);
            }
            _farmLevel = rootVisualElement.Q<SliderInt>(nameof(UIDocumentNames.SliderInt_Farm));
            if (_farmLevel != null)
            {
               _farmLevel.RegisterValueChangedCallback(SliderIntFarmValue);
            }
            _plantLevel = rootVisualElement.Q<SliderInt>(nameof(UIDocumentNames.SliderInt_Plant));
            if (_plantLevel != null)
            {
               _plantLevel.RegisterValueChangedCallback(SliderIntPlantValue);
            }
            _specialLevel = rootVisualElement.Q<SliderInt>(nameof(UIDocumentNames.SliderInt_Special));
            if (_specialLevel != null)
            {
               _specialLevel.RegisterValueChangedCallback(SliderIntSpecialValue);
            }

            #endregion

            #region Wall

            _wallNone = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_WallNone));
            if (_wallNone != null)
            {
               _wallNone.RegisterValueChangedCallback(ToggleWallNoneValue);
            }
            _wallYes = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_WallYes));
            if (_wallYes != null)
            {
               _wallYes.RegisterValueChangedCallback(ToggleWallYesValue);
            }
            _wallNo = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_WallNo));
            if (_wallNo != null)
            {
               _wallNo.RegisterValueChangedCallback(ToggleWallNoValue);
            }

            #endregion

            #region Save/Load Buttons

            var saveButton = rootVisualElement.Q<Button>(nameof(UIDocumentNames.Button_Save));
            if (saveButton != null)
            {
               saveButton.clicked += () => { SaveEvent.Invoke(); };
            }

            var loadButton = rootVisualElement.Q<Button>(nameof(UIDocumentNames.Button_Load));
            if (loadButton != null)
            {
               loadButton.clicked += () => { LoadEvent.Invoke(); };
            }

            #endregion

            #region Edit Mode
            _editMode = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_EditMode));
            if (_editMode != null)
            {
               _editMode.RegisterValueChangedCallback(ToggleEditValue);
            }
            #endregion

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

      #region Urban

      void ToggleUrbanValue(ChangeEvent<bool> evt)
      {
         UrbanToggle.Invoke(evt.newValue);
      }

      void SliderIntUrbanValue(ChangeEvent<int> evt)
      {
         UrbanSlider.Invoke(evt.newValue);
      }

      #endregion

      #region Farm

      void ToggleFarmValue(ChangeEvent<bool> evt)
      {
         FarmToggle.Invoke(evt.newValue);
      }

      void SliderIntFarmValue(ChangeEvent<int> evt)
      {
         FarmSlider.Invoke(evt.newValue);
      }

      #endregion

      #region Plant

      void TogglePlantValue(ChangeEvent<bool> evt)
      {
         PlantToggle.Invoke(evt.newValue);
      }

      void SliderIntPlantValue(ChangeEvent<int> evt)
      {
         PlantSlider.Invoke(evt.newValue);
      }

      #endregion

      #region Special

      void ToggleSpecialValue(ChangeEvent<bool> evt)
      {
         SpecialToggle.Invoke(evt.newValue);
      }

      void SliderIntSpecialValue(ChangeEvent<int> evt)
      {
         SpecialSlider.Invoke(evt.newValue);
      }

      #endregion

      #region Walled

      void ToggleWallNoneValue(ChangeEvent<bool> evt)
      {
         if (evt.newValue)
         {
            WallModeChanged.Invoke(0);
            _wallNo.SetValueWithoutNotify(false);
            _wallYes.SetValueWithoutNotify(false);
         }
      }

      void ToggleWallYesValue(ChangeEvent<bool> evt)
      {
         if (evt.newValue)
         {
            WallModeChanged.Invoke(1);
            _wallNo.SetValueWithoutNotify(false);
            _wallNone.SetValueWithoutNotify(false);
         }
         else
         {
            WallModeChanged.Invoke(0);
         }
      }

      void ToggleWallNoValue(ChangeEvent<bool> evt)
      {
         if (evt.newValue)
         {
            WallModeChanged.Invoke(2);
            _wallNone.SetValueWithoutNotify(false);
            _wallYes.SetValueWithoutNotify(false);
         }
         else
         {
            WallModeChanged.Invoke(0);
         }
      }

      #endregion

      void ToggleEditValue(ChangeEvent<bool> evt)
      {
         EditModeToggle.Invoke(evt.newValue);
      }

      public enum UIDocumentNames
      {
         #region Features
         Toggle_None,
         Toggle_Sand,
         Toggle_Grass,
         Toggle_Mud,
         Toggle_Stone,
         Toggle_Snow,
         #endregion

         Toggle_Elevation,
         Toggle_Water,

         Toggle_RiverYes,
         Toggle_RiverNo,
         Toggle_RiverNone,
         Toggle_RoadYes,
         Toggle_RoadNo,
         Toggle_RoadNone,
         Toggle_WallNone,
         Toggle_WallYes,
         Toggle_WallNo,
         Toggle_Urban,
         Toggle_Farm,
         Toggle_Plant,
         Toggle_Special,
         Toggle_EditMode,

         SliderInt_Elevation,
         SliderInt_Water,
         SliderInt_BrushSize,
         SliderInt_Urban,
         SliderInt_Plant,
         SliderInt_Farm,
         SliderInt_Special,

         Button_Save,
         Button_Load
      }
   }
}