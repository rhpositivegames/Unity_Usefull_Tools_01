using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace DefaultFolders
{
    public class FolderCreator : EditorWindow
    {
        List<Folder> folders;

        [MenuItem("/Tools/Folder Creator")]
        private static void ShowWindow()
        {
            var window = GetWindow<FolderCreator>();
            window.titleContent = new GUIContent("FolderCreator");
            window.minSize = new Vector2(250, 200);
            window.Show();
        }

        void Awake()
        {
            SetFolderNames();
        }

        void SetFolderNames()
        {
            folders = new List<Folder>();

            CraeteFolder("Animations", new string[] { "UI" });
            CraeteFolder("Images", new string[] { "Backgrounds", "Temp", "UI" });
            CraeteFolder("Fonts", new string[] { "SDF", "Temp" });
            CraeteFolder("Materials", new string[] { "Particles", "Physics", "Temp" });
            CraeteFolder("Meshes", new string[] { "Temp" });
            CraeteFolder("Prefabs", new string[] { "Levels", "Particles", "UI" });
            CraeteFolder("Scenes", new string[] { "Temp" });
            CraeteFolder("Scripts", new string[] { "Ads", "Camera", "Contants", "Data", "Firebase", "Game", "General", "Menu", "Sounds", "Splash", "Temp", "UI" });
            CraeteFolder("Sounds", new string[] { "Music", "Sounds" });
            CraeteFolder("Textures", new string[] { "Particles", "Temp" });
        }

        void CraeteFolder(string rootName, string[] subFolderName)
        {
            Folder folder = new Folder();
            folder.ROOT_NAME = rootName;
            folder.SUB_FOLDER_NAMES = subFolderName;
            folder.ROOT_ENABLE = true;
            folder.SUB_FOLDERS_ENABLE = new bool[subFolderName.Length];
            for (int i = 0; i < subFolderName.Length; i++)
            {
                folder.SUB_FOLDERS_ENABLE[i] = !subFolderName[i].Equals("Temp");
            }
            folders.Add(folder);
        }

        Vector2 scrollPosition;
        private void OnGUI()
        {
            EditorGUILayout.Space(5f);
            GUIStyle style = GetStyle(null, TextAnchor.MiddleCenter, 16, FontStyle.Bold, Color.cyan);
            EditorGUILayout.LabelField("Folders And Subfolders", style);
            EditorGUILayout.Space(5f);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, "box");
            for (int rootCount = 0; rootCount < folders.Count; rootCount++)
            {
                folders[rootCount].ROOT_ENABLE = EditorGUILayout.BeginToggleGroup(folders[rootCount].ROOT_NAME, folders[rootCount].ROOT_ENABLE);
                for (int subFolder = 0; subFolder < folders[rootCount].SUB_FOLDERS_ENABLE.Length; subFolder++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(50);
                    folders[rootCount].SUB_FOLDERS_ENABLE[subFolder] = EditorGUILayout.Toggle(folders[rootCount].SUB_FOLDER_NAMES[subFolder], folders[rootCount].SUB_FOLDERS_ENABLE[subFolder]);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndToggleGroup();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create Folders", GUILayout.Width(160)))
            {
                CreateFolders();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
        }

        void CreateFolders()
        {
            for (int i = 0; i < folders.Count; i++)
            {
                if (folders[i].ROOT_ENABLE == false)
                    continue;

                if (AssetDatabase.IsValidFolder("Assets/" + folders[i].ROOT_NAME) == false)
                    AssetDatabase.CreateFolder("Assets", folders[i].ROOT_NAME);

                for (int j = 0; j < folders[i].SUB_FOLDERS_ENABLE.Length; j++)
                {
                    if (folders[i].SUB_FOLDERS_ENABLE[j] == false)
                        continue;

                    if (AssetDatabase.IsValidFolder("Assets/" + folders[i].ROOT_NAME + "/" + folders[i].SUB_FOLDER_NAMES[j]) == false)
                        AssetDatabase.CreateFolder("Assets/" + folders[i].ROOT_NAME, folders[i].SUB_FOLDER_NAMES[j]);
                }
            }
        }

        GUIStyle GetStyle(GUIStyle gUIStyle, TextAnchor alingment, int fontSize, FontStyle fontStyle, Color color)
        {
            GUIStyle style = gUIStyle != null ? new GUIStyle(gUIStyle) : new GUIStyle();
            style.alignment = alingment;
            if (fontSize != -1) style.fontSize = fontSize;
            style.fontStyle = fontStyle;
            style.normal.textColor = color;
            return style;
        }
    }

    [Serializable]
    public class Folder
    {
        public string ROOT_NAME;
        public string[] SUB_FOLDER_NAMES;
        public bool ROOT_ENABLE;
        public bool[] SUB_FOLDERS_ENABLE;
    }
}