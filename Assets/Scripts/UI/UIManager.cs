using HexMap.Map;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HexMap.UI
{
   public class UIManager : MonoBehaviour
   {
      HexMapEditor _hexMapEditor = default;

      [Header("Screens")]
      [SerializeField] UIMapEdit _mapEditorMenu = default;
      [SerializeField] UISaveLoadMenu _saveLoadMenu = default;

      void Awake()
      {
         // Find HexMapEditor which controls all functionality
         _hexMapEditor = GetComponent<HexMapEditor>();

         if (_hexMapEditor == null)
         {
            throw new Exception("HexMapEditor is missing!");
         }
      }

      void Start()
      {
         if (_mapEditorMenu != null)
         {
            _mapEditorMenu.gameObject.SetActive(true);

            RegisterCallbacks_MapEdit(true);
            RegisterCallbacks_SaveLoadMenu(true);
         }
         else
         {
            Debug.LogError("_mapEditorMenu was not found.");
         }
      }

      void OnDestroy()
      {
         RegisterCallbacks_MapEdit(false);
         RegisterCallbacks_SaveLoadMenu(false);
      }

      #region UI

      public void OpenSaveLoadMenu()
      {
         _mapEditorMenu.gameObject.SetActive(false);
         _saveLoadMenu.gameObject.SetActive(true);
      }

      public void CloseSaveLoadMenu()
      {
         _mapEditorMenu.gameObject.SetActive(true);
         _saveLoadMenu.gameObject.SetActive(false);
      }

      #region Event Callbacks

      void RegisterCallbacks_MapEdit(bool action)
      {
         if (_mapEditorMenu != null)
         {
            // Register vs Unregister
            if (action)
            {
               _mapEditorMenu.FeatureToggleChanged += _hexMapEditor.SetTerrainTypeIndex;

               _mapEditorMenu.ElevationSlider += _hexMapEditor.SetElevation;
               _mapEditorMenu.ElevationToggle += _hexMapEditor.SetApplyElevation;
               _mapEditorMenu.WaterSlider += _hexMapEditor.SetWaterLevel;
               _mapEditorMenu.WaterToggle += _hexMapEditor.SetApplyWaterLevel;
               _mapEditorMenu.RiverModeChanged += _hexMapEditor.SetRiverMode;
               _mapEditorMenu.RoadModeChanged += _hexMapEditor.SetRoadMode;
               _mapEditorMenu.BrushSizeSlider += _hexMapEditor.SetBrushSize;

               _mapEditorMenu.UrbanToggle += _hexMapEditor.SetApplyUrbanLevel;
               _mapEditorMenu.FarmToggle += _hexMapEditor.SetApplyFarmLevel;
               _mapEditorMenu.PlantToggle += _hexMapEditor.SetApplyPlantLevel;
               _mapEditorMenu.SpecialToggle += _hexMapEditor.SetApplySpecialIndex;

               _mapEditorMenu.UrbanSlider += _hexMapEditor.SetUrbanLevel;
               _mapEditorMenu.FarmSlider += _hexMapEditor.SetFarmLevel;
               _mapEditorMenu.PlantSlider += _hexMapEditor.SetPlantLevel;
               _mapEditorMenu.SpecialSlider += _hexMapEditor.SetSpecialIndex;

               _mapEditorMenu.WallModeChanged += _hexMapEditor.SetWalledMode;

               _mapEditorMenu.EditModeToggle += _hexMapEditor.SetEditMode;

               _mapEditorMenu.SaveEvent += OpenSaveLoadMenu;
               _mapEditorMenu.LoadEvent += OpenSaveLoadMenu;
            }
            else
            {
               _mapEditorMenu.FeatureToggleChanged -= _hexMapEditor.SetTerrainTypeIndex;

               _mapEditorMenu.ElevationSlider -= _hexMapEditor.SetElevation;
               _mapEditorMenu.ElevationToggle -= _hexMapEditor.SetApplyElevation;
               _mapEditorMenu.WaterSlider -= _hexMapEditor.SetWaterLevel;
               _mapEditorMenu.WaterToggle -= _hexMapEditor.SetApplyWaterLevel;
               _mapEditorMenu.RiverModeChanged -= _hexMapEditor.SetRiverMode;
               _mapEditorMenu.RoadModeChanged -= _hexMapEditor.SetRoadMode;
               _mapEditorMenu.BrushSizeSlider -= _hexMapEditor.SetBrushSize;

               _mapEditorMenu.UrbanToggle -= _hexMapEditor.SetApplyUrbanLevel;
               _mapEditorMenu.FarmToggle -= _hexMapEditor.SetApplyFarmLevel;
               _mapEditorMenu.PlantToggle -= _hexMapEditor.SetApplyPlantLevel;
               _mapEditorMenu.SpecialToggle -= _hexMapEditor.SetApplySpecialIndex;

               _mapEditorMenu.UrbanSlider -= _hexMapEditor.SetUrbanLevel;
               _mapEditorMenu.FarmSlider -= _hexMapEditor.SetFarmLevel;
               _mapEditorMenu.PlantSlider -= _hexMapEditor.SetPlantLevel;
               _mapEditorMenu.SpecialSlider -= _hexMapEditor.SetSpecialIndex;

               _mapEditorMenu.WallModeChanged -= _hexMapEditor.SetWalledMode;

               _mapEditorMenu.EditModeToggle -= _hexMapEditor.SetEditMode;

               _mapEditorMenu.SaveEvent -= OpenSaveLoadMenu;
               _mapEditorMenu.LoadEvent -= OpenSaveLoadMenu;
            }

         }
      }

      void RegisterCallbacks_SaveLoadMenu(bool action)
      {
         if (_saveLoadMenu != null)
         {
            if (action)
            {
               _saveLoadMenu.CloseDocument += CloseSaveLoadMenu;
            }
            else
            {
               _saveLoadMenu.CloseDocument -= CloseSaveLoadMenu;
            }
         }
      }


      #endregion

      #endregion
   }
}