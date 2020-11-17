using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Data;
using ExcelDataReader;
using DFP;

namespace ExcelToJSon
{
    public class Excel2JSon : EditorWindow
    {
        FileStream fileStream;
        DataSet result;
        Dictionary<int, Type[,]> cellTypes;

        string path;
        string openedFileName = " Not Selected.";
        string description = "";

        bool isFileLoaded;
        bool isUseSerializeAttribute = true;
        bool isUseFForFloat = true;

        [MenuItem("/Tools/Excel To JSon")]
        private static void ShowWindow()
        {
            var window = GetWindow<Excel2JSon>();
            window.titleContent = new GUIContent("Excel2JSon");
            window.minSize = new Vector2(270, 300);
            window.maxSize = window.minSize;
            window.Show();
        }

        private void OnGUI()
        {
            CreateLabel();
            CreateGUI();
        }

        void CreateLabel()
        {
            EditorGUILayout.Space(5f);
            GUIStyle style = GetStyle(null, TextAnchor.MiddleCenter, 16, FontStyle.Bold, Color.cyan);
            EditorGUILayout.LabelField("Excel To JSON", style);
            EditorGUILayout.Space(5f);
        }

        void CreateGUI()
        {

            GUIStyle style = GetStyle(null, TextAnchor.MiddleLeft, -1, FontStyle.Bold, Color.white);
            EditorGUILayout.BeginHorizontal("box", GUILayout.Width(position.width - 6), GUILayout.Height(60));
            EditorGUILayout.BeginVertical();
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Excel File: ", style, GUILayout.Width(60));
            style = GetStyle(null, TextAnchor.MiddleLeft, -1, FontStyle.Normal, Color.yellow);
            EditorGUILayout.LabelField(openedFileName, style, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Load\nExcel File", GUILayout.Width(160)))
            {
                LoadExcelFile();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = isFileLoaded;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Convert To\nJSON", GUILayout.Width(120)))
            {
                if (isFileLoaded)
                {
                    description = "Creating JSON file..";
                    Repaint();
                    ConvertToJSon();
                }
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Convert To\nC#", GUILayout.Width(120)))
            {
                if (isFileLoaded)
                {
                    description = "Creating C# file..";
                    Repaint();
                    ConvertToCSharp();
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            isUseSerializeAttribute = EditorGUILayout.Toggle(isUseSerializeAttribute, GUILayout.Width(20));
            EditorGUILayout.LabelField("Use \"Serializable\" attribute");
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            isUseFForFloat = EditorGUILayout.Toggle(isUseFForFloat, GUILayout.Width(20));
            EditorGUILayout.LabelField("Convert cells ending in \"f\" to \"float\"");
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal(EditorStyles.textArea, GUILayout.Width(position.width - 6), GUILayout.Height(60));
            style = GetStyle(null, TextAnchor.MiddleLeft, -1, FontStyle.Normal, new Color(255f / 255f, 171f / 255f, 110f / 255f));
            EditorGUILayout.LabelField(description, style);
            EditorGUILayout.EndHorizontal();
        }

        void LoadExcelFile()
        {
            path = EditorUtility.OpenFilePanel("Select Excel File", "", "xls,xlsx");
            if (path.Length != 0)
            {
                try
                {
                    fileStream = File.Open(path, FileMode.Open, FileAccess.Read); //Dosya Kullanımdaysa hata veriyor..
                    openedFileName = Path.GetFileNameWithoutExtension(path);
                    Debug.Log(openedFileName);
                    description = "File loaded.";
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                    Debug.Log("Close excel file.");
                    return;
                }

                isFileLoaded = true;
                IExcelDataReader reader = ExcelReaderFactory.CreateReader(fileStream);
                result = reader.AsDataSet();

                cellTypes = new Dictionary<int, Type[,]>();

                for (int workSheetIndex = 0; workSheetIndex < reader.ResultsCount; workSheetIndex++)
                {
                    Type[,] types = new Type[reader.RowCount, reader.FieldCount];
                    int rowIndex = 0;
                    while (reader.Read())
                    {
                        for (int columnIndex = 0; columnIndex <= reader.FieldCount - 1; columnIndex++)
                        {
                            types[rowIndex, columnIndex] = reader.GetFieldType(columnIndex);
                        }
                        rowIndex++;
                    }
                    cellTypes.Add(workSheetIndex, types);
                    reader.NextResult();
                }

                reader.Close();
                reader.Dispose();
                reader = null;
            }
        }

        void ConvertToJSon()
        {
            string TEXTJSON = "{\r\n";

            string comma;
            List<string> headers;
            string items = "";

            for (int tableIndex = 0; tableIndex < result.Tables.Count; tableIndex++)
            {
                TEXTJSON += "\t\"" + result.Tables[tableIndex].TableName + "\": [\r\n";

                var table = result.Tables[tableIndex];

                headers = new List<string>();
                int totalColumns = table.Columns.Count;

                for (int columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
                {
                    if (table.Rows[0][columnIndex].ToString().Equals("") || table.Rows[0][columnIndex].ToString() == null)
                    {
                        totalColumns = columnIndex;
                        break;
                    }
                    headers.Add("\"" + table.Rows[0][columnIndex] + "\": ");
                }

                items = "";

                for (int rowIndex = 1; rowIndex < table.Rows.Count; rowIndex++)
                {
                    items += "\t\t{\r\n";
                    for (int columnIndex = 0; columnIndex < totalColumns; columnIndex++)
                    {
                        comma = "";
                        if (columnIndex < totalColumns - 1)
                        {
                            comma += ",";
                        }
                        string text = "\t\t\t" + headers[columnIndex] + GetFixedType(tableIndex, table, rowIndex, columnIndex) + comma + "\r\n";
                        items += text;
                    }
                    comma = "";
                    if (rowIndex < table.Rows.Count - 1)
                    {
                        comma += ",";
                    }
                    items += "\t\t}" + comma + "\r\n";
                }

                comma = "";
                if (tableIndex < result.Tables.Count - 1)
                {
                    comma += ",";
                }
                TEXTJSON += items + "\t]" + comma + "\r\n";
            }

            TEXTJSON += "}";

            string fileName = path + "/JSonData.txt";

            try
            {
                SaveJSon(TEXTJSON);
                description = "\nJSON file created. \n(Assets/Excel To JSon/Created Files)";
                Debug.Log("Converted!");
            }
            catch (Exception ex)
            {
                description = ex.ToString();
                Debug.Log(ex);
            }
        }

        void SaveJSon(string TEXTJSON)
        {
            if (AssetDatabase.IsValidFolder("Assets/Excel To JSon/Created Files") == false)
                AssetDatabase.CreateFolder("Assets/Excel To JSon", "Created Files");

            string pathJSON = "Assets/Excel To JSon/Created Files/" + openedFileName + "_JSON.txt";

            File.WriteAllText(pathJSON, TEXTJSON);
            AssetDatabase.Refresh();
        }

        string GetFixedType(int tableIndex, DataTable table, int rowIndex, int columnIndex)
        {
            string text = "";
            Type type = cellTypes[tableIndex][rowIndex, columnIndex];
            string target = "" + table.Rows[rowIndex][columnIndex];

            if (type.Equals(typeof(System.String)))
            {
                if (target.Substring(target.Length - 1).Equals("f") && isUseFForFloat)
                {
                    float v = 0;
                    if (float.TryParse(target.Substring(0, target.Length - 1), out v))
                    {
                        text = target.Substring(0, target.Length - 1);
                    }
                    else
                    {
                        text = "\"" + table.Rows[rowIndex][columnIndex] + "\"";
                    }
                }
                else
                {
                    text = "\"" + table.Rows[rowIndex][columnIndex] + "\"";
                }
            }

            if (type.Equals(typeof(System.Double)))
            {
                text = "" + table.Rows[rowIndex][columnIndex];
            }

            if (type.Equals(typeof(System.DateTime)))
            {
                text = "" + table.Rows[rowIndex][columnIndex];
            }

            return text;
        }

        void ConvertToCSharp()
        {
            string serializeText = "";
            if (isUseSerializeAttribute)
            {
                serializeText = "[Serializable]\r\n";
            }

            string TEXTCSHARP = "";

            string items = "";

            for (int tableIndex = 0; tableIndex < result.Tables.Count; tableIndex++)
            {
                items = serializeText;
                items += "public class " + Utils.Singularize(result.Tables[tableIndex].TableName) + "\r\n{";

                var table = result.Tables[tableIndex];

                int totalColumns = table.Columns.Count;
                for (int columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
                {
                    if (table.Rows[0][columnIndex].ToString().Equals("") || table.Rows[0][columnIndex].ToString() == null)
                    {
                        totalColumns = columnIndex;
                        break;
                    }
                }

                for (int columnIndex = 0; columnIndex < totalColumns; columnIndex++)
                {
                    items += "\r\n\tpublic " + GetVariableType(tableIndex, table, columnIndex) + " " + table.Rows[0][columnIndex] + ";";
                }

                items += "\r\n}";
                TEXTCSHARP += items + "\r\n\r\n";
            }

            items = serializeText + "public class RootObject\r\n{\r\n";
            for (int tableIndex = 0; tableIndex < result.Tables.Count; tableIndex++)
            {
                items += "\tpublic List<" + Utils.Singularize(result.Tables[tableIndex].TableName) + "> " + result.Tables[tableIndex].TableName + ";\r\n";
            }

            items += "}";
            TEXTCSHARP += items;

            string fileName = path + "/CSharpData.txt";

            try
            {
                SaveCSharp(TEXTCSHARP);
                description = "\nC# file created. \n(Assets/Excel To JSon/Created Files)";
            }
            catch (Exception ex)
            {
                description = ex.ToString();
                Debug.Log(ex);
            }
        }

        void SaveCSharp(string TEXTCSHARP)
        {
            if (AssetDatabase.IsValidFolder("Assets/Excel To JSon/Created Files") == false)
                AssetDatabase.CreateFolder("Assets/Excel To JSon", "Created Files");

            string pathCSHARP = "Assets/Excel To JSon/Created Files/" + openedFileName + "_CSHARP.txt";

            File.WriteAllText(pathCSHARP, TEXTCSHARP);
            AssetDatabase.Refresh();
        }

        string GetVariableType(int tableIndex, DataTable table, int columnIndex)
        {
            Type type = cellTypes[tableIndex][1, columnIndex];

            if (table.Rows[1][columnIndex] == null)
                return "object";

            string target = "" + table.Rows[1][columnIndex];

            if (type.Equals(typeof(System.String)))
            {
                if (target.Substring(target.Length - 1).Equals("f") && isUseFForFloat)
                {
                    float v = 0;
                    if (float.TryParse(target.Substring(0, target.Length - 1), out v))
                    {
                        return "float";
                    }
                    else
                    {
                        return "string";
                    }
                }
                else
                {
                    return "string";
                }
            }

            if (type.Equals(typeof(System.Double)))
            {
                return "int";
            }

            if (type.Equals(typeof(System.DateTime)))
            {
                return "DateTime";
            }

            return "Object";
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
}
