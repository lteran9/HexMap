using System;
using HexMap.Map;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;

namespace HexMap.UI
{
   public class UINewMapMenu : MonoBehaviour
   {
      // Root of all functionality
      UIDocument _uiDocument = default;

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
               _generate.RegisterValueChangedCallback(evt =>
               {

               });
            }

            _smallBtn = rootVisualElement.Q<Button>(nameof(UIDocumentNames.Button_Small));
            if (_smallBtn != null)
            {
               _smallBtn.clicked += () => { CreateSmallMap(); };
            }

            _mediumBtn = rootVisualElement.Q<Button>(nameof(UIDocumentNames.Button_Small));
            if (_mediumBtn != null)
            {
               _mediumBtn.clicked += () => { CreateMediumMap(); };
            }

            _largeBtn = rootVisualElement.Q<Button>(nameof(UIDocumentNames.Button_Small));
            if (_largeBtn != null)
            {
               _largeBtn.clicked += () => { CreateLargeMap(); };
            }

            _cancelBtn = rootVisualElement.Q<Button>(nameof(UIDocumentNames.Button_Small));
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

      void CreateSmallMap()
      {
         CreateMap(20, 15);
      }

      void CreateMediumMap()
      {
         CreateMap(40, 30);
      }

      void CreateLargeMap()
      {
         CreateMap(80, 60);
      }

      void ToggleMapGeneration(bool toggle)
      {
         generateMaps = toggle;
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