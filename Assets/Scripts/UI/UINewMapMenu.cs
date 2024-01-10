using System;
using HexMap.Map;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;

namespace HexMap.UI {
   public class UINewMapMenu : BaseUIWindow {
      #region Actions

      public event UnityAction CancelButtonEvent = delegate { };

      #endregion

      #region Toggle

      private Toggle _generate = default;

      #endregion

      #region Button

      private Button _smallBtn = default;
      private Button _mediumBtn = default;
      private Button _largeBtn = default;
      private Button _cancelBtn = default;

      #endregion

      private bool generateMaps = false;

      [SerializeField] private HexGrid _hexGrid = default;
      [SerializeField] private HexMapGenerator _mapGenerator = default;

      private void Awake() {
         _uiDocument = GetComponent<UIDocument>();
         if (_uiDocument == null) {
            Debug.LogError(string.Format("{0}: Unable to find associated UIDocument component.", nameof(UINewMapMenu)));
         }
      }

      private void OnEnable() {
         VisualElement rootVisualElement = _uiDocument?.rootVisualElement;

         if (rootVisualElement != null) {
            _generate = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_Generate));
            if (_generate != null) {
               _generate.RegisterValueChangedCallback(ToggleMapGeneration);
            }

            _smallBtn = rootVisualElement.Q<Button>(nameof(UIDocumentNames.Button_Small));
            if (_smallBtn != null) {
               _smallBtn.clicked += Click_SmallMap;
            }

            _mediumBtn = rootVisualElement.Q<Button>(nameof(UIDocumentNames.Button_Medium));
            if (_mediumBtn != null) {
               _mediumBtn.clicked += Click_MediumMap;
            }

            _largeBtn = rootVisualElement.Q<Button>(nameof(UIDocumentNames.Button_Large));
            if (_largeBtn != null) {
               _largeBtn.clicked += Click_LargeMap;
            }

            _cancelBtn = rootVisualElement.Q<Button>(nameof(UIDocumentNames.Button_Cancel));
            if (_cancelBtn != null) {
               _cancelBtn.clicked += () => { CancelButtonEvent.Invoke(); };
            }
         }
      }

      private void CreateMap(int x, int z) {
         if (generateMaps) {
            _mapGenerator.GenerateMap(x, z);
         } else {
            _hexGrid.CreateMap(x, z);
         }
         // CameraManager.ValidatePosition();
         CancelButtonEvent.Invoke();
      }

      private void Click_SmallMap() {
         CreateMap(20, 15);
      }

      private void Click_MediumMap() {
         CreateMap(40, 30);
      }

      private void Click_LargeMap() {
         CreateMap(80, 60);
      }

      private void Click_Cancel() {
         CancelButtonEvent.Invoke();
      }

      private void ToggleMapGeneration(ChangeEvent<bool> toggle) {
         generateMaps = toggle.newValue;
      }

      private enum UIDocumentNames {
         Toggle_Generate,
         Button_Small,
         Button_Medium,
         Button_Large,
         Button_Cancel
      }
   }
}