using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace SimpleSaveLoad
{
    public class SaveLoad : MonoBehaviour
    {
        public bool resetOnStart = false;
        public bool isTest = false;
        public bool isAllOpen = false;

        private static SaveLoad _instance;

        public static SaveLoad Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(this.gameObject);
            SetFirstData();
        }

        #region SAVE LOAD METHODS

        public string GetGeneral(string description)
        {
            return PlayerPrefs.GetString(description);
        }

        public void SetGeneral(string description, string value)
        {
            PlayerPrefs.SetString(description, value);
        }

        public int GetInt(string description)
        {
            return PlayerPrefs.GetInt(description);
        }

        public void SetInt(string description, int value)
        {
            PlayerPrefs.SetInt(description, value);
        }

        public float GetFloat(string description)
        {
            return PlayerPrefs.GetFloat(description);
        }

        public void SetFloat(string description, float value)
        {
            PlayerPrefs.SetFloat(description, value);
        }

        public void SetBool(string name, bool booleanValue)
        {
            PlayerPrefs.SetInt(name, booleanValue ? 1 : 0);
        }

        public bool GetBool(string name)
        {
            return PlayerPrefs.GetInt(name) == 1 ? true : false;
        }

        public void SaveList<T>(string description, List<T> list)
        {
            JSonList<T> jSonList = new JSonList<T>();
            jSonList.list = list;
            string text = JsonUtility.ToJson(jSonList);
            PlayerPrefs.SetString(description, text);
        }

        public List<T> LoadList<T>(string description)
        {
            JSonList<T> jSonList = JsonUtility.FromJson<JSonList<T>>(PlayerPrefs.GetString(description));
            List<T> list = jSonList.list;
            return list;
        }

        public void SaveData<T>(T data, string fileName)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            string path = Application.persistentDataPath + fileName + ".dat";
            try
            {
                FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
                formatter.Serialize(stream, data);
                stream.Close();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Save Error: " + e);
            }
        }

        public T LoadData<T>(string fileName)
        {
            string path = Application.persistentDataPath + fileName + ".dat";
            if (File.Exists(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Open);
                T data = (T)formatter.Deserialize(stream);
                stream.Close();
                return data;
            }
            else
            {
                Debug.LogError("File not found!");
                return default(T);
            }

        }

        public void CLearAllData()
        {
            PlayerPrefs.DeleteAll();
        }

        public void ResetGame()
        {
            SetBool(Settings.IS_FIRST_DATA_LOAD, false);
            SetFirstData();
        }

        #endregion

        public void SetFirstData()
        {
            bool isFirstDataLoaded = GetBool(Settings.IS_FIRST_DATA_LOAD);

            if (isFirstDataLoaded && resetOnStart == false)
            {
                return;
            }

            // Standart data save (Unique save name,value)
            SetInt(Settings.VERSION, 0);
            SetBool(Settings.IS_SOUND_ON, true);
            SetBool(Settings.IS_MUSIC_ON, true);

            // Class save (class, Unique save file name)
            PlayerData data = new PlayerData();
            data.CURRENT_LEVEL = 0;
            data.TOTAL_GOLD = 0;
            SaveData<PlayerData>(data, FileNames.PLAYER_DATA_NAME);

            isFirstDataLoaded = true;

            Test();
            AllOpen();
        }

        void Test()
        {
            if (isTest == false)
                return;

            // Change values for test
        }

        public void AllOpen()
        {
            if (isAllOpen == false)
                return;

            // Open all data
        }
    }

    // ======================================================================

    [System.Serializable]
    public class JSonList<T>
    {
        [SerializeField]
        public List<T> list;
    }

    [System.Serializable]
    public class PlayerData
    {
        public int CURRENT_LEVEL;
        public int TOTAL_GOLD;
    }

    // ======================================================================

    public class FileNames
    {
        public static string PLAYER_DATA_NAME = "/plyr";
    }
}