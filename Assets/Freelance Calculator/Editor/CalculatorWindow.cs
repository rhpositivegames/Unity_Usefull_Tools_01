using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

namespace Freelance
{
    [ExecuteInEditMode]
    public class CalculatorWindow : EditorWindow
    {
        List<bool> periodFoldouts = new List<bool>();
        WorksData data;
        string dataPath = "Assets/Freelance Calculator/";
        string estimatedTime = "00:00";
        double periodTimeStart;
        double pauseTimeStart;
        double pausedTime;
        string pausedWarningText = "";

        bool isPeriodStarted = false;
        bool isPausePeriod = false;
        Vector2 scrollPosition = Vector2.zero;

        [MenuItem("Freelance/Calculator")]
        private static void ShowWindow()
        {
            var window = GetWindow<CalculatorWindow>();
            window.titleContent = new GUIContent("Freelance");
            window.minSize = new Vector2(320, 200);
            window.Show();
        }

        bool isDataChecked = false;
        private void OnEnable()
        {
            EditorApplication.quitting -= OnDestroy;
            EditorApplication.quitting += OnDestroy;

            CraeteScriptableObject();
            EditorUtility.SetDirty(data);
            CheckData();
            SetPeriodFoldouts();
        }

        private void OnDestroy() // Not Working when close Unity!!!
        {
            if (isPeriodStarted == false)
                return;

            EndPeriod();
        }

        private void OnGUI()
        {
            CreateLabel();

            if (data != null)
            {
                CreateTotalWorksTime();
                CreateFinanceTable();
                CreateExtraTime();
                CreatePeriods();
                CreateEstimatedTime();
                CreateButtons();
            }
        }

        void CreateLabel()
        {
            EditorGUILayout.Space(5f);
            GUIStyle style = GetStyle(null, TextAnchor.MiddleCenter, 20, FontStyle.Bold, Color.cyan);
            EditorGUILayout.LabelField("Works", style);
        }

        void CreateTotalWorksTime()
        {
            GUIStyle style = GetStyle(null, TextAnchor.MiddleCenter, 12, FontStyle.Bold, Color.cyan);
            EditorGUILayout.LabelField(GetTotalHours(), style);
            EditorGUILayout.Space(5f);
        }

        void CreateFinanceTable()
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("$", GUILayout.Width(10));
            GUIStyle styleHour = GetStyle(EditorStyles.textField, TextAnchor.MiddleRight, -1, FontStyle.Bold, Color.yellow);
            EditorGUI.BeginChangeCheck();
            data.hourlyWage = EditorGUILayout.FloatField(data.hourlyWage, styleHour, GUILayout.Width(40));
            if (EditorGUI.EndChangeCheck())
            {
                data.totalRevenue = CalculateRevenue();
                Repaint();
            }
            EditorGUILayout.LabelField("/hr", GUILayout.Width(20));
            GUILayout.FlexibleSpace();
            GUIStyle styleRevenue = GetStyle(EditorStyles.textField, TextAnchor.MiddleRight, -1, FontStyle.Bold, Color.cyan);
            EditorGUILayout.LabelField("Total Revenue", GUILayout.Width(90));
            EditorGUILayout.TextField(data.totalRevenue.ToString(), styleRevenue, GUILayout.Width(50));
            EditorGUILayout.LabelField("$", GUILayout.Width(10));
            EditorGUILayout.EndHorizontal();

            Repaint();
        }

        void CreateExtraTime()
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Extra Time (+/-)");
            GUILayout.FlexibleSpace();
            GUIStyle styleHour = GetStyle(EditorStyles.textField, TextAnchor.MiddleRight, -1, FontStyle.Bold, Color.yellow);
            EditorGUI.BeginChangeCheck();
            data.extraTime = EditorGUILayout.IntField(data.extraTime, styleHour, GUILayout.Width(60));
            if (EditorGUI.EndChangeCheck())
            {
                data.totalRevenue = CalculateRevenue();
                Repaint();
            }
            EditorGUILayout.LabelField(" min.", GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5f);
        }

        void CreatePeriods()
        {
            if (data.days == null || data.days.Count == 0)
                return;

            GUIStyle styleLeft = GetStyle(EditorStyles.textField, TextAnchor.MiddleRight, -1, FontStyle.Normal, Color.gray);
            GUIStyle styleMinute = GetStyle(EditorStyles.textField, TextAnchor.MiddleRight, -1, FontStyle.Normal, Color.white);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < data.days.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");
                Day day = data.days[i];
                int totalMinutes = 0;
                for (int m = 0; m < day.minutes.Count; m++)
                {
                    totalMinutes += day.minutes[m];
                }
                int hr = totalMinutes / 60;
                int min = totalMinutes % 60;
                periodFoldouts[i] = EditorGUILayout.Foldout(periodFoldouts[i], DateToString(StringToDate(day.date), DateType.DATE) + " (" + hr.ToString("00") + ":" + min.ToString("00") + ")");
                if (periodFoldouts[i])
                {
                    for (int k = 0; k < day.startTime.Count; k++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("     Period " + k + "  ", GUILayout.Width(80));

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.TextField(DateToString(StringToDate(day.startTime[k]), DateType.TIME), styleLeft, GUILayout.Width(50));
                        EditorGUILayout.LabelField(" - ", GUILayout.Width(10));
                        string endTimeText;
                        string minutes;
                        if (k == day.endTime.Count)
                        {
                            endTimeText = "";
                            minutes = "";
                        }
                        else
                        {
                            endTimeText = DateToString(StringToDate(day.endTime[k]), DateType.TIME);
                            minutes = "" + day.minutes[k];
                        }
                        EditorGUILayout.TextField(endTimeText, styleLeft, GUILayout.Width(50));
                        EditorGUILayout.LabelField("", GUILayout.Width(10));
                        EditorGUILayout.TextField(minutes, styleMinute, GUILayout.Width(30));
                        EditorGUILayout.LabelField(" min.", GUILayout.Width(30));
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

        void CreateEstimatedTime()
        {
            EditorGUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUIStyle style = GetStyle(null, TextAnchor.MiddleCenter, 14, FontStyle.Bold, Color.cyan);
            EditorGUILayout.LabelField(estimatedTime, style, GUILayout.Width(60));
            if (isPausePeriod)
            {
                style = GetStyle(null, TextAnchor.MiddleCenter, 14, FontStyle.Bold, Color.yellow);
                EditorGUILayout.LabelField(pausedWarningText, style, GUILayout.Width(60));
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5f);
        }

        void CreateButtons()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (isPeriodStarted == false)
            {
                if (GUILayout.Button("Start New Period", GUILayout.Width(160)))
                {
                    StartNewPeriod();
                }
            }
            else
            {
                if (GUILayout.Button(isPausePeriod ? "Resume" : "Pause", GUILayout.Width(140)))
                {
                    PausePeriod();
                }

                EditorGUILayout.Space(10);
                if (GUILayout.Button("End Period", GUILayout.Width(140)))
                {
                    EndPeriod();
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);


        }

        void StartNewPeriod()
        {
            if (data.days.Count == 0 || !StringToDate(data.days[data.days.Count - 1].date).Day.Equals(DateTime.Now.Day)
            || !StringToDate(data.days[data.days.Count - 1].date).Month.Equals(DateTime.Now.Month)
            || !StringToDate(data.days[data.days.Count - 1].date).Year.Equals(DateTime.Now.Year))
            {
                Day day = new Day();
                day.date = DateToString(DateTime.Now, DateType.FULL);
                day.startTime.Add(DateToString(DateTime.Now, DateType.FULL));
                data.days.Add(day);
                if (periodFoldouts.Count > 0) periodFoldouts[periodFoldouts.Count - 1] = false;
                periodFoldouts.Add(true);
            }
            else
            {
                Day day = data.days[data.days.Count - 1];
                day.startTime.Add(DateToString(DateTime.Now, DateType.FULL));
            }

            periodFoldouts[periodFoldouts.Count - 1] = true;
            isPeriodStarted = true;
            periodTimeStart = EditorApplication.timeSinceStartup;
            pausedTime = 0;
            Repaint();
        }

        void PausePeriod()
        {
            isPausePeriod = !isPausePeriod;

            if (isPausePeriod)
            {
                pauseTimeStart = EditorApplication.timeSinceStartup;
            }
            else
            {
                pausedTime += EditorApplication.timeSinceStartup - pauseTimeStart;
            }
        }

        void EndPeriod()
        {
            if (isPausePeriod)
                PausePeriod();

            Day day = data.days[data.days.Count - 1];
            day.endTime.Add(DateToString(DateTime.Now, DateType.FULL));
            int seconds = (int)(StringToDate(day.endTime[day.endTime.Count - 1]) - StringToDate(day.startTime[day.startTime.Count - 1])).TotalSeconds - (int)pausedTime;
            int minutes = seconds / 60;
            day.minutes.Add(minutes);
            data.totalRevenue = CalculateRevenue();
            isPeriodStarted = false;
            estimatedTime = "00:00";
            SaveBackup();
            Repaint();
        }

        float CalculateRevenue()
        {
            int totalMinutes = GetTotalMinutes();
            return (int)(data.hourlyWage / 60 * totalMinutes);
        }

        void CraeteScriptableObject()
        {
            WorksData[] w = GetAllInstances<WorksData>();

            for (int i = 0; i < w.Length; i++)
            {
                if (AssetDatabase.GetAssetPath(w[i]).Equals(dataPath + "Data.asset"))
                {
                    data = w[i];
                    return;
                }
            }

            data = ScriptableObject.CreateInstance<WorksData>();
            data.days = new List<Day>();
            AssetDatabase.CreateAsset(data, "Assets/Freelance Calculator/Data.asset");
            AssetDatabase.SaveAssets();
        }

        public static T[] GetAllInstances<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            T[] a = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return a;
        }

        void CheckData()
        {
            if (isDataChecked)
                return;

            for (int i = 0; i < data.days.Count; i++)
            {
                Day day = data.days[i];
                if (i == data.days.Count - 1)
                {
                    int startTimeCount = data.days[i].startTime.Count;
                    int endTimeCount = data.days[i].endTime.Count;
                    int minutesCount = data.days[i].minutes.Count;
                    int completedItemCount = Mathf.Min(new int[] { startTimeCount, endTimeCount, minutesCount });
                    for (int k = startTimeCount - 1; k >= completedItemCount; k--)
                    {
                        data.days[i].startTime.RemoveAt(k);
                    }
                    for (int k = endTimeCount - 1; k >= completedItemCount; k--)
                    {
                        data.days[i].endTime.RemoveAt(k);
                    }
                    for (int k = minutesCount - 1; k >= completedItemCount; k--)
                    {
                        data.days[i].minutes.RemoveAt(k);
                    }
                }

            }

            isDataChecked = true;
        }

        void SetPeriodFoldouts()
        {
            if (data.days != null)
            {
                periodFoldouts.Clear();
                for (int i = 0; i < data.days.Count; i++)
                {
                    if (i == data.days.Count - 1)
                        periodFoldouts.Add(true);
                    else
                        periodFoldouts.Add(false);
                }
            }
        }

        void SaveBackup()
        {
            if (AssetDatabase.IsValidFolder("Assets/Freelance Calculator/Backup") == false)
                AssetDatabase.CreateFolder("Assets/Freelance Calculator", "Backup");

            string path = dataPath + "Backup/backup.txt";

            if (!File.Exists(path))
            {
                string createText = "Backup" + Environment.NewLine;
                File.WriteAllText(path, createText);
            }

            Day day = data.days[data.days.Count - 1];
            string date = DateToString(StringToDate(day.date), DateType.DATE);
            string startTime = DateToString(StringToDate(day.startTime[day.startTime.Count - 1]), DateType.TIME);
            string endTime = DateToString(StringToDate(day.endTime[day.endTime.Count - 1]), DateType.TIME);
            string backupText = date + "____" + startTime + "____" + endTime + "      (" + day.minutes[day.minutes.Count - 1] + " min.)" + Environment.NewLine;
            File.AppendAllText(path, backupText);
            AssetDatabase.Refresh();
        }

        int GetTotalMinutes()
        {
            int totalMinutes = 0;
            for (int i = 0; i < data.days.Count; i++)
            {
                for (int k = 0; k < data.days[i].minutes.Count; k++)
                {
                    totalMinutes += data.days[i].minutes[k];
                }
            }
            totalMinutes += data.extraTime;
            totalMinutes = Mathf.Clamp(totalMinutes, 0, totalMinutes);
            return totalMinutes;
        }

        string GetTotalHours()
        {
            int totalMinutes = GetTotalMinutes();
            int hr = totalMinutes / 60;
            int min = totalMinutes % 60;
            return "(" + hr.ToString("00") + " hr. " + min.ToString("00") + " min.)";
        }

        string DateToString(DateTime date, DateType dateType)
        {
            string st = "";
            switch (dateType)
            {
                case DateType.FULL:
                    st = date.ToString("dd/MM/yyyy HH:mm:ss");
                    break;
                case DateType.DATE:
                    st = date.ToString("dd/MM/yyyy");
                    break;
                case DateType.TIME:
                    st = date.ToString("HH:mm");
                    break;
                default:
                    break;
            }
            return st;
        }

        DateTime StringToDate(string st)
        {
            return DateTime.ParseExact(st, "dd/MM/yyyy HH:mm:ss", null);
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

        void Update()
        {
            if (isPeriodStarted == false)
                return;

            CalculatePeriodTime();
        }

        void CalculatePeriodTime()
        {
            if (isPausePeriod)
            {
                int sec = (int)EditorApplication.timeSinceStartup % 2;
                pausedWarningText = sec % 2 == 0 ? "  PAUSED!" : "";
                return;
            }

            int seconds = (int)(EditorApplication.timeSinceStartup - periodTimeStart - pausedTime);
            int hours = seconds / 3600;
            int minutes = (seconds / 60) % 60;

            estimatedTime = hours.ToString("00") + ":" + minutes.ToString("00");
        }

    }

    enum DateType
    {
        FULL,
        DATE,
        TIME
    }
}
