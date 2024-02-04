using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using HexMap.Map;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;

namespace HexMap.UI {
   [RequireComponent(typeof(UIDocument))]
   public class UISaveLoadMenu : BaseUIWindow {
      const int mapFileVersion = 0;

      #region Actions

      public event UnityAction CloseDocument = delegate { };

      #endregion

      #region Button
      Button _save = default;
      Button _load = default;
      Button _cancel = default;
      #endregion

      #region List View
      ListView _mapNamesList = default;
      #endregion

      #region Text Field
      TextField _fileName = default;
      #endregion

      string inputFileName = default;

      List<string> pathNames = default;

      [SerializeField] HexGrid _hexGrid = default;

      void Awake() {
         _uiDocument = GetComponent<UIDocument>();
      }

      void OnEnable() {
         VisualElement rootVisualElement = _uiDocument?.rootVisualElement;

         if (rootVisualElement != null) {
            _save = rootVisualElement.Q<Button>(nameof(UIDocumentNames.Button_Save));
            _load = rootVisualElement.Q<Button>(nameof(UIDocumentNames.Button_Load));
            _cancel = rootVisualElement.Q<Button>(nameof(UIDocumentNames.Button_Cancel));

            _mapNamesList = rootVisualElement.Q<ListView>(nameof(UIDocumentNames.ListView_FileNames));
            _mapNamesList.makeItem = MakeListView;
            _mapNamesList.bindItem = BindListViewItem;
            _mapNamesList.itemsSource = pathNames = new List<string>();
            _mapNamesList.fixedItemHeight = 50f;
            _mapNamesList.onSelectionChange += FileName_Selected;

            _fileName = rootVisualElement.Q<TextField>(nameof(UIDocumentNames.TextField_FileName));

            if (_save != null)
               _save.clicked += Click_Save;
            if (_load != null)
               _load.clicked += Click_Load;
            if (_cancel != null)
               _cancel.clicked += Click_Cancel;
            if (_fileName != null)
               _fileName.RegisterValueChangedCallback(FileName_TextField_Changed);

            FillList();
         }
      }

      void Click_Save() {
         if (!string.IsNullOrEmpty(inputFileName)) {
            Save(GetSelectedPath());
            Click_Cancel();
         }
      }

      void Click_Load() {
         if (!string.IsNullOrEmpty(inputFileName)) {
            Load(GetSelectedPath());
            Click_Cancel();
         }
      }

      void Click_Cancel() {
         CloseDocument.Invoke();
      }

      void FileName_TextField_Changed(ChangeEvent<string> evt) {
         if (!string.IsNullOrWhiteSpace(evt.newValue) && evt.newValue.Length > 3) {
            inputFileName = evt.newValue;
         }
      }

      #region List View 

      void FillList() {
         if (_mapNamesList.childCount > 0) {
            _mapNamesList.Clear();
         }

         string[] paths = Directory.GetFiles(Application.persistentDataPath, "*.map");
         Array.Sort(paths);
         foreach (var path in paths) {
            if (path.Length > 0) {
               var sections = path.Replace("\\", "/").Replace(".map", "").Split("/");
               pathNames.Add(sections[sections.Length - 1]);
            }
         }

         _mapNamesList?.RefreshItems();
      }

      void FileName_Selected(IEnumerable<object> selection) {
         if (selection?.Any() == true) {
            _fileName.SetValueWithoutNotify((string)selection.First());
            inputFileName = _fileName.text;
         }
      }

      VisualElement MakeListView() {
         var label = new Label();
         label.name = "Label_PathName";
         label.style.fontSize = 18f;
         label.style.unityFontStyleAndWeight = FontStyle.Bold;
         label.style.flexGrow = 1f;
         label.style.paddingBottom = 10f;
         label.style.paddingLeft = 10f;
         label.style.paddingRight = 10f;
         label.style.paddingTop = 10f;
         label.style.marginTop = label.style.marginBottom = label.style.marginLeft = label.style.marginRight = 0f;
         label.style.unityTextAlign = TextAnchor.MiddleLeft;

         var btnDelete = new Button();
         btnDelete.name = "Button_DeleteMap";
         btnDelete.text = "X";
         btnDelete.style.fontSize = 18f;
         btnDelete.style.flexShrink = 1f;
         btnDelete.style.flexGrow = 0f;
         btnDelete.style.paddingBottom = btnDelete.style.paddingLeft = btnDelete.style.paddingRight = btnDelete.style.paddingTop = 0f;
         btnDelete.style.marginBottom = btnDelete.style.marginLeft = btnDelete.style.marginRight = btnDelete.style.marginTop = 10f;
         btnDelete.style.borderBottomWidth = btnDelete.style.borderLeftWidth = btnDelete.style.borderRightWidth = btnDelete.style.borderTopWidth = 0f;
         btnDelete.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0f));
         btnDelete.clicked += () => {
            var path = GetSelectedPath();
            Delete(path);
            for (int i = 0; i < pathNames.Count; i++) {
               if (path.Contains(pathNames[i])) {
                  pathNames.RemoveAt(i);
                  break;
               }
            }

            _mapNamesList.Rebuild();
            _fileName.value = string.Empty;
         };

         var container = new VisualElement();
         container.style.flexDirection = FlexDirection.Row;
         container.Add(label);
         container.Add(btnDelete);

         return container;
      }

      void BindListViewItem(VisualElement e, int index) {
         //We add the game name to the label of the list item
         e.Q<Label>("Label_PathName").text = pathNames[index];
      }

      #endregion

      #region Data Storage

      void Save(string path) {
         using (var writer = new BinaryWriter(File.Open(path, FileMode.Create))) {
            writer.Write(mapFileVersion);
            _hexGrid.Save(writer);
         }

      }

      void Load(string path) {
         if (!File.Exists(path)) {
            Debug.LogError("File does not exist " + path);
            return;
         }

         using (var reader = new BinaryReader(File.OpenRead(path))) {
            int header = reader.ReadInt32();
            if (header <= mapFileVersion) {
               _hexGrid.Load(reader, header);
               //CameraManager.ValidatePosition();
            } else {
               Debug.LogWarning("Unknown map format " + header);
            }
         }
      }

      void Delete(string path) {
         if (!string.IsNullOrEmpty(path)) {
            File.Delete(path);
         } else {
            Debug.Log("Given path is NULL.");
         }
      }

      string GetSelectedPath() {
         string mapName = inputFileName;
         if (mapName.Length == 0) {
            return null;
         }

         return Path.Combine(Application.persistentDataPath, mapName + ".map");
      }

      #endregion

      enum UIDocumentNames {
         Button_Save,
         Button_Load,
         Button_Cancel,
         ListView_FileNames,
         TextField_FileName
      }
   }
}