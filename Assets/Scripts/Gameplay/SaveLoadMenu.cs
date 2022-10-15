using HexMap.Map;
using HexMap.Gameplay;
using System;
using System.IO;
using TMPro;
using UnityEngine;

public class SaveLoadMenu : MonoBehaviour
{
   bool saveMode = default;

   [SerializeField] TMP_InputField _fileName = default;
   [SerializeField] TextMeshProUGUI _menuLabel = default, _actionButtonLabel = default;
   [SerializeField] HexGrid _hexGrid = default;
   [SerializeField] RectTransform _listContent = default;
   [SerializeField] SaveLoadItem _itemPrefab = default;

   public void Open(bool saveMode)
   {
      this.saveMode = saveMode;
      if (saveMode)
      {
         _menuLabel.text = "Save Map";
         _actionButtonLabel.text = "Save";
      }
      else
      {
         _menuLabel.text = "Load Map";
         _actionButtonLabel.text = "Load";
      }
      FillList();
      gameObject.SetActive(true);
      CameraManager.Locked = true;
   }

   public void Close()
   {
      gameObject.SetActive(false);
      CameraManager.Locked = false;
   }

   public void Action()
   {
      string path = GetSelectedPath();
      if (path == null)
      {
         return;
      }
      if (saveMode)
      {
         Save(path);
      }
      else
      {
         Load(path);
      }
   }

   public void SelectItem(string name)
   {
      _fileName.text = name;
   }

   public void Delete()
   {
      string path = GetSelectedPath();
      if (path == null)
      {
         return;
      }
      if (File.Exists(path))
      {
         File.Delete(path);
      }
      _fileName.text = "";
      FillList();
   }

   void FillList()
   {
      for (int i = 0; i < _listContent.childCount; i++)
      {
         Destroy(_listContent.GetChild(i).gameObject);
      }

      string[] paths = Directory.GetFiles(Application.persistentDataPath, "*.map");
      Array.Sort(paths);
      foreach (var path in paths)
      {
         SaveLoadItem item = Instantiate(_itemPrefab);
         item.menu = this;
         item.MapName = Path.GetFileNameWithoutExtension(path);
         item.transform.SetParent(_listContent, false);
      }
   }

   #region Data Storage

   void Save(string path)
   {
      using (var writer = new BinaryWriter(File.Open(path, FileMode.Create)))
      {
         writer.Write(2);
         _hexGrid.Save(writer);
      }

   }

   void Load(string path)
   {
      if (!File.Exists(path))
      {
         Debug.LogError("File does not exist " + path);
         return;
      }

      using (var reader = new BinaryReader(File.OpenRead(path)))
      {
         int header = reader.ReadInt32();
         if (header <= 2)
         {
            _hexGrid.Load(reader, header);
            CameraManager.ValidatePosition();
         }
         else
         {
            Debug.LogWarning("Unknown map format " + header);
         }
      }
   }

   string GetSelectedPath()
   {
      string mapName = _fileName.text;
      if (mapName.Length == 0)
      {
         return null;
      }

      return Path.Combine(Application.persistentDataPath, mapName + ".map");
   }

   #endregion
}
