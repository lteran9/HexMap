using System;
using HexMap.Map;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;

namespace HexMap.UI
{
   public class UINewMapMenu : BaseUIWindow
   {
      #region Actions

      public event UnityAction CancelButtonEvent = delegate { };

      #endregion

      #region Toggle

      Toggle _generate = default;

      #endregion

      #region Button

      Button _smallBtn = default;
      Button _mediumBtn = default;
      Button _largeBtn = default;
      Button _cancelBtn = default;

      #endregion

      bool generateMaps = false;

      [SerializeField] HexGrid _hexGrid = default;
      [SerializeField] HexMapGenerator _mapGenerator = default;

      void Awake()
      {
         _uiDocument = GetComponent<UIDocument>();
         if (_uiDocument == null)
         {
            Debug.LogError(string.Format("{0}: Unable to find associated UIDocument component.", nameof(UINewMapMenu)));
         }
      }

      void OnEnable()
      {
         VisualElement rootVisualElement = _uiDocument?.rootVisualElement;

         if (rootVisualElement != null)
         {
            _generate = rootVisualElement.Q<Toggle>(nameof(UIDocumentNames.Toggle_Generate));
            if (_generate != null)
            {
               _generate.RegisterValueChangedCallback(ToggleMapGeneration);
            }

            _smallBtn = rootVisualElement.Q<Button>(nameof(UIDocumentNames.Button_Small));
            if (_smallBtn != null)
            {
               _smallBtn.clicked += Click_SmallMap;
            }

            _mediumBtn = rootVisualElement.Q<Button>(nameof(UIDocumentNames.Button_Medium));
            if (_mediumBtn != null)
            {
               _mediumBtn.clicked += Click_MediumMap;
            }

            _largeBtn = rootVisualElement.Q<Button>(nameof(UIDocumentNames.Button_Large));
            if (_largeBtn != null)
            {
               _largeBtn.clicked += Click_LargeMap;
            }

            _cancelBtn = rootVisualElement.Q<Button>(nameof(UIDocumentNames.Button_Cancel));
            if (_cancelBtn != null)
            {
               _cancelBtn.clicked += () => { CancelButtonEvent.Invoke(); };
            }
         }
      }

      void CreateMap(int x, int z)
      {
         if (generateMaps)
         {
            _mapGenerator.GenerateMap(x, z);
         }
         else
         {
            _hexGrid.CreateMap(x, z);
         }
         // CameraManager.ValidatePosition();
         CancelButtonEvent.Invoke();
      }

      void Click_SmallMap()
      {
         CreateMap(20, 15);
      }

      void Click_MediumMap()
      {
         CreateMap(40, 30);
      }

      void Click_LargeMap()
      {
         CreateMap(80, 60);
      }

      void Click_Cancel()
      {
         CancelButtonEvent.Invoke();
      }

      void ToggleMapGeneration(ChangeEvent<bool> toggle)
      {
         generateMaps = toggle.newValue;
      }

      enum UIDocumentNames
      {
         Toggle_Generate,
         Button_Small,
         Button_Medium,
         Button_Large,
         Button_Cancel
      }
   }
}