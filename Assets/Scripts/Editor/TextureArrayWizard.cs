using UnityEditor;
using UnityEngine;

public class TextureArrayWizard : ScriptableWizard
{
   [MenuItem("Assets/Create/Texture Array")]
   static void CreateWizard()
   {
      ScriptableWizard.DisplayWizard<TextureArrayWizard>(
         "Create Texture Array", "Create"
      );
   }

   public Texture2D[] _textures;

   void OnWizardCreate()
   {
      if (_textures.Length == 0)
      {
         return;
      }
      string path = EditorUtility.SaveFilePanelInProject(
         "Save Texture Array", "Texture Array", "asset", "Save Texture Array"
      );
      if (path.Length == 0)
      {
         return;
      }

      Texture2D tex = _textures[0];
      Texture2DArray textureArray = new Texture2DArray(
         tex.width, tex.height, _textures.Length, tex.format, tex.mipmapCount > 1
      );
      textureArray.anisoLevel = tex.anisoLevel;
      textureArray.filterMode = tex.filterMode;
      textureArray.wrapMode = tex.wrapMode;

      for (int i = 0; i < _textures.Length; i++)
      {
         for (int m = 0; m < tex.mipmapCount; m++)
         {
            Graphics.CopyTexture(_textures[i], 0, m, textureArray, i, m);
         }
      }

      AssetDatabase.CreateAsset(textureArray, path);
   }
}
