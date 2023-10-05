using UnityEditor;
using UnityEngine;
using VirtueSky.DataStorage;

namespace VirtueSky.EditorControl
{
#if UNITY_EDITOR
    public class WindowControl : EditorWindow
    {
        [MenuItem("GameControl/Clear Data %F1")]
        public static void ClearAllData()
        {
            GameData.DelDataInStorage();
            GameData.Clear();
            PlayerPrefs.DeleteAll();
            Debug.Log($"<color=Green>Clear data succeed</color>");
        }

        [MenuItem("GameControl/Save Data %F2")]
        public static void SaveData()
        {
            GameData.Save();
        }
    }
#endif
}