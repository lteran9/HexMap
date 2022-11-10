using System.Collections.Generic;
using HexMap.Map;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HexMap.UI
{
   public class UIManager : MonoBehaviour
   {
      [SerializeField] HexMapEditor _hexMapEditor = default;

      [Header("Screens")]
      [SerializeField] UIMapEdit _mapEditorMenu = default;
      [SerializeField] UISaveLoadMenu _saveLoadMenu = default;
      [SerializeField] UINewMapMenu _newMapMenu = default;

      void Awake()
      {
         if (_hexMapEditor == null)
         {
            throw new Exception(string.Format("{0} is missing!", nameof(HexMapEditor)));
         }
      }

      void Start()
      {
         if (_mapEditorMenu != null)
         {
            ShowDefaultMenu();
            RegisterCallbacks_MapEdit(true);
            RegisterCallbacks_SaveLoadMenu(true);
            RegisterCallbacks_NewMapMenu(true);
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
         RegisterCallbacks_NewMapMenu(false);
      }

      #region UI

      public void ShowDefaultMenu()
      {
         _mapEditorMenu.gameObject.SetActive(true);
      }

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

      public void OpenNewMapMenu()
      {
         _mapEditorMenu.gameObject.SetActive(false);
         _newMapMenu.gameObject.SetActive(true);
      }

      public void CloseNewMapMenu()
      {
         _mapEditorMenu.gameObject.SetActive(true);
         _newMapMenu.gameObject.SetActive(false);
      }

      #region Event Callbacks

      void RegisterCallbacks_MapEdit(bool register)
      {
         if (_mapEditorMenu != null)
         {
            // Register vs Unregister
            if (register)
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

               _mapEditorMenu.GridModeToggle += _hexMapEditor.ShowGrid;
               _mapEditorMenu.EditModeToggle += _hexMapEditor.SetEditMode;

               _mapEditorMenu.SaveEvent += OpenSaveLoadMenu;
               _mapEditorMenu.LoadEvent += OpenSaveLoadMenu;
               _mapEditorMenu.NewMapEvent += OpenNewMapMenu;
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

               _mapEditorMenu.GridModeToggle -= _hexMapEditor.ShowGrid;
               _mapEditorMenu.EditModeToggle -= _hexMapEditor.SetEditMode;

               _mapEditorMenu.SaveEvent -= OpenSaveLoadMenu;
               _mapEditorMenu.LoadEvent -= OpenSaveLoadMenu;
               _mapEditorMenu.NewMapEvent -= OpenNewMapMenu;
            }

         }
      }

      void RegisterCallbacks_SaveLoadMenu(bool register)
      {
         if (_saveLoadMenu != null)
         {
            if (register)
            {
               _saveLoadMenu.CloseDocument += CloseSaveLoadMenu;
            }
            else
            {
               _saveLoadMenu.CloseDocument -= CloseSaveLoadMenu;
            }
         }
      }

      void RegisterCallbacks_NewMapMenu(bool register)
      {
         if (_newMapMenu != null)
         {
            if (register)
            {
               _newMapMenu.CancelButtonEvent += CloseNewMapMenu;
            }
            else
            {
               _newMapMenu.CancelButtonEvent -= CloseNewMapMenu;
            }
         }
      }

      #endregion

      #endregion
   }
}