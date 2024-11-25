using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using Excel;
using System;
using System.Data;

public class ExcelTool : Editor
{
    public class ExcelClassData
    {
        public string excelName; //表名
        public string className; //类名
        public string[] infos;    //注释
        public string[] types;    //类型
        public string[] propertyName;    //属性名
        public List<string[]> datas; //数据
    }
    

    const string File_Path = "L:/A_Learn/UnityExcelTools/config"; // excel表路径，根据自己的存放路径修改
    const string Asset_Path = "Assets/Resources/Config/ConfigAsset.asset";
    const string Byte_Path = "Assets/Resources/Config/Byte";

    [MenuItem("ExcelTool/生成数据")]
    static void ExcelToData()
    {
        //所有xlsx表格
        string[] files = Directory.GetFiles(File_Path, "*.xlsx");

        //所有数据信息
        List<ExcelClassData> allClassDatas = new List<ExcelClassData>();

        for (int i = 0; i < files.Length; i++)
        {
            string file = files[i];
            string excelName = Path.GetFileNameWithoutExtension(file);
            if (excelName.StartsWith("~")) continue;

            // 打开文件流
            FileStream fileStream = File.Open(file, FileMode.Open, FileAccess.Read);
            // 创建Excel数据阅读器
            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);
            if (!excelReader.IsValid)
            {
                Debug.Log("读取excel失败" + file);
                continue;
            }

            // 读取数据为DataSet（如果只需要单个工作表的数据，可以使用AsDataSet(false)）
            DataSet dataSet = excelReader.AsDataSet(false);
            // 获取第一个工作表（通常为0索引）
            DataTable dataTable = dataSet.Tables[0];
            // 访问第4行数据
            DataRow fourRow = dataTable.Rows[3];
            List<string> clients = new List<string>();
            // 读取数据
            foreach (DataColumn column in dataTable.Columns)
            {
                object cellValue = fourRow[column.ColumnName];
                clients.Add(cellValue.ToString());
            }

            //构建数据
            ExcelClassData exdata = new ExcelClassData();
            exdata.excelName = excelName;
            exdata.className = excelReader.Name;
            exdata.datas = new List<string[]>();

            int line = 1;
            while (excelReader.Read())
            {
                //一行数据
                int len = excelReader.FieldCount;
                List<string> list = new List<string>();
                for (int j = 0; j < len; j++)
                {
                    var client = clients[j];
                    if (client.Equals("s") || client.Equals("")) continue;
                    var val = excelReader.GetString(j);
                    list.Add(val);
                }
                if (list[0] == null) break;
                string[] strLineDatas = list.ToArray();

                //注释行
                if (line == 1)
                {
                    exdata.infos = strLineDatas;
                }
                //类型行
                else if (line == 2)
                {
                    exdata.types = strLineDatas;
                }
                //属性名行
                else if (line == 3)
                {
                    strLineDatas[0] = "id"; // 项目规范，可删除
                    exdata.propertyName = strLineDatas;
                }
                //数据行
                else if (line > 4)
                {
                    exdata.datas.Add(strLineDatas);
                }

                line++;
            }

            allClassDatas.Add(exdata);
        }

        //写出数据和脚本操作
        Writer(allClassDatas);
        AddConfigAsset();
        AssetDatabase.Refresh();
        // 新增的表格第一次会没添加到ConfigAsset上，所以多执行一遍
        AddConfigAsset();
        AssetDatabase.Refresh();

        Debug.Log("----->excel datas trans finish!");
    }

    static void AddConfigAsset()
    {
        ConfigAsset configAsset = AssetDatabase.LoadAssetAtPath<ConfigAsset>(Asset_Path);
        if (configAsset == null)
        {
            Debug.LogError("ConfigAsset not found!");
            return;
        }

        string[] bytesFilePaths = Directory.GetFiles(Byte_Path, "*.bytes");

        configAsset.configs = new TextAsset[bytesFilePaths.Length];

        for (int i = 0; i < bytesFilePaths.Length; i++)
        {
            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(bytesFilePaths[i]);
            if (textAsset != null)
            {
                configAsset.configs[i] = textAsset;
            }
        }

        EditorUtility.SetDirty(configAsset);
        AssetDatabase.SaveAssets();
    }

    static void Writer(List<ExcelClassData> exDataList)
    {
        #region//---bytes---
        StringBuilder utilContentSb = new StringBuilder();
        StringBuilder utilConditionSb = new StringBuilder();
        for (int i = 0; i < exDataList.Count; i++)
        {
            ExcelClassData exData = exDataList[i];

            List<byte> byteList = new List<byte>();

            int dataCount = exData.datas.Count;
            byteList.AddRange(PickData.WriteInt(dataCount));

            int tempIndex = 0;
            while (tempIndex < dataCount)
            {
                string[] data = exData.datas[tempIndex];
                for (int j = 0; j < exData.types.Length; j++)
                {
                    if (j == 0 && data[j] == null) break;
                    byte[] tbytes = GetBytes(exData.types[j], data[j]);
                    byteList.AddRange(tbytes);
                }
                tempIndex++;
            }

            //bytes数据文件生成
            string savePath = Application.dataPath + "/Resources/Config/Byte/" + exData.className + ".bytes";
            File.WriteAllBytes(savePath, byteList.ToArray());

            //脚本生成
            string saveCodePath = Application.dataPath + "/Scripts/Runtime/Config/Excel";
            if (!Directory.Exists(saveCodePath)) Directory.CreateDirectory(saveCodePath);
            string clsName = exData.className + "Cfg";
            string strCode = CreateCode(clsName, exData.types, exData.propertyName, exData.infos);
            File.WriteAllText(saveCodePath + "/" + clsName + ".cs", strCode);

            utilContentSb.Append(CreateUtilContentCode(exData.excelName, exData.className));
            utilConditionSb.Append(CreateUtilConditionCode(exData.className));
        }

        string saveUtilPath = Application.dataPath + "/Scripts/Runtime/Config/Utils";
        if (!Directory.Exists(saveUtilPath)) Directory.CreateDirectory(saveUtilPath);
        string utilCode = CreateUtilCode(utilContentSb.ToString(), utilConditionSb.ToString());
        File.WriteAllText(saveUtilPath + "/ConfigUtils.cs", utilCode);
        #endregion

        #region//---json---
        //AssemblyName assemblyName = new AssemblyName("dynamicAssembly");
        //AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        //ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

        //for (int i = 0; i < exDataList.Count; i++)
        //{
        //    ExcelClassData exData = exDataList[i];
        //    //定义类型
        //    TypeBuilder typeBuilder = moduleBuilder.DefineType(exData.className, TypeAttributes.Public);
        //    //定义属性
        //    for (int j = 0; j < exData.types.Length; j++)
        //    {
        //        typeBuilder.DefineField(exData.propertyName[j], GetType(exData.types[j]), FieldAttributes.Public);
        //    }
        //    //t
        //    Type t = typeBuilder.CreateType();

        //    List<object> allObjList = new List<object>();
        //    for (int j = 0; j < exData.datas.Count; j++)
        //    {
        //        //一行数据
        //        string[] strDatas = exData.datas[j];
        //        //反射实例
        //        object obj = Activator.CreateInstance(t);
        //        for (int k = 0; k < exData.types.Length; k++)
        //        {
        //            //设置属性值
        //            FieldInfo fieldInfo = t.GetField(exData.propertyName[k]);
        //            object value = GetValue(exData.types[k], strDatas[k]);
        //            fieldInfo.SetValue(obj, value);
        //        }
        //        allObjList.Add(obj);
        //    }
        //    string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(allObjList, Newtonsoft.Json.Formatting.Indented);
        //    string dataFloder = Application.streamingAssetsPath + "/Datas";
        //    if (!System.IO.Directory.Exists(dataFloder))
        //    {
        //        Directory.CreateDirectory(dataFloder);
        //    }
        //    File.WriteAllText(dataFloder + "/" + exData.className + ".json", jsonData);
        //}
        #endregion
    }

    /// <summary>
    /// 获取类型
    /// </summary>
    static Type GetType(string typeName)
    {
        switch (typeName)
        {
            case "int":
                return typeof(int);
            case "float":
                return typeof(float);
            case "string":
                return typeof(string);
            case "int[]":
                return typeof(int[]);
            case "float[]":
                return typeof(float[]);
            case "string[]":
                return typeof(string[]);
                //default:
                //    return null;
        }
        return null;
    }

    /// <summary>
    /// 获取数据Obj
    /// </summary>
    static object GetValue(string typeName, string data)
    {
        var len = 0;
        string[] ss = null;
        if (data != null)
        {
            ss = data.Split('|');
            len = ss.Length;
        }

        switch (typeName)
        {
            case "int":
                return data != null ? int.Parse(data) : 0;
            case "float":
                return data != null ? float.Parse(data) : 0f;
            case "string":
                return data ?? "";
            case "int[]":
                int[] intArray = new int[len];
                for (int i = 0; i < len; i++)
                {
                    intArray[i] = int.Parse(ss[i]);
                }
                return intArray;
            case "float[]":
                float[] floatArray = new float[len];
                for (int i = 0; i < len; i++)
                {
                    floatArray[i] = float.Parse(ss[i]);
                }
                return floatArray;
            case "string[]":
                return ss ?? new string[0];
                //default:
                //    return null;
        }
        return null;
    }

    /// <summary>
    /// 获取数据Obj的Bytes
    /// </summary>
    static byte[] GetBytes(string typeName, string data)
    {
        List<byte> bytes = new List<byte>();
        object obj = GetValue(typeName, data);
        switch (typeName)
        {
            case "int":
                bytes.AddRange(PickData.WriteInt((int)obj));
                break;
            case "float":
                bytes.AddRange(PickData.WriteFloat((float)obj));
                break;
            case "string":
                bytes.AddRange(PickData.WriteString((string)obj));
                break;
            case "int[]":
                bytes.AddRange(PickData.WriteIntArray((int[])obj));
                break;
            case "float[]":
                bytes.AddRange(PickData.WriteFloatArray((float[])obj));
                break;
            case "string[]":
                bytes.AddRange(PickData.WriteStringArray((string[])obj));
                break;
            default:
                break;
        }
        return bytes.ToArray();
    }


    /// <summary>
    /// 生成代码
    /// </summary>
    static string CreateCode(string className, string[] types, string[] names, string[] texts)
    {
        string tmpPath = Application.dataPath + "/Scripts/Editor/Config/CfgTmp.txt";
        string tmpStr = GetTemplate(tmpPath);
        string replacedTmp = tmpStr.Replace("{{className}}", className);

        StringBuilder stringBuilder = new StringBuilder();
        //属性定义
        for (int i = 0; i < types.Length; i++)
        {
            //注释
            stringBuilder.Append(StrNotes(texts[i], 2));
            //定义
            stringBuilder.Append("\t\tpublic " + types[i] + " " + ConvertFirstChar(names[i]) + ";\n");
        }
        replacedTmp = replacedTmp.Replace("{{content}}", stringBuilder.ToString());

        stringBuilder.Length = 0;

        //---bytes解析---
        for (int i = 0; i < types.Length; i++)
        {
            string readInfo = "";
            switch (types[i])
            {
                case "int":
                    readInfo = "PickData.ReadInt";
                    break;
                case "float":
                    readInfo = "PickData.ReadFloat";
                    break;
                case "string":
                    readInfo = "PickData.ReadString";
                    break;
                case "int[]":
                    readInfo = "PickData.ReadIntArray";
                    break;
                case "float[]":
                    readInfo = "PickData.ReadFloatArray";
                    break;
                case "string[]":
                    readInfo = "PickData.ReadStringArray";
                    break;
                default:
                    break;
            }
            stringBuilder.Append("\t\t\t" + ConvertFirstChar(names[i]) + " = " + readInfo + "(datas, ref index);\n");
        }
        stringBuilder.Append("\t\t\tuid = Id;");
        replacedTmp = replacedTmp.Replace("{{parseData}}", stringBuilder.ToString());

        return replacedTmp;
    }

    /// <summary>
    /// 生成ConfigUtils.cs代码
    /// </summary>
    static string CreateUtilCode(string content, string condition)
    {
        string tmpPath = Application.dataPath + "/Scripts/Editor/Config/ConfigUtilsTmp.txt";
        string tmpStr = GetTemplate(tmpPath);
        string replacedTmp = tmpStr.Replace("{{content}}", content);
        replacedTmp = replacedTmp.Replace("{{condition}}", condition);
        return replacedTmp;
    }

    static string CreateUtilContentCode(string excelName, string className)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(StrNotes(excelName, 2));
        stringBuilder.Append("\t\tpublic static Dictionary<int, PickData> " + className + "s { get { return GetConfig(\"" + className + "\"); } }\n");
        return stringBuilder.ToString();
    }

    static string CreateUtilConditionCode(string className)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("\t\t\t\tcase \"" + className + "\":\n");
        stringBuilder.Append("\t\t\t\t\t_cfgDict.Add(name, Convert(ParseData<" + className + "Cfg>(datas)));\n");
        stringBuilder.Append("\t\t\t\t\tbreak;\n");
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 注释
    /// </summary>
    static string StrNotes(string tip, int t = 0)
    {
        StringBuilder stringBuilder = new StringBuilder();
        string st = "";
        for (int i = 0; i < t; i++)
        {
            st += "\t";
        }
        stringBuilder.Append(st + "/// <summary>\n");
        stringBuilder.Append(st + "/// " + tip + "\n");
        stringBuilder.Append(st + "/// <summary>\n");
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 获取代码模板
    /// </summary>
    /// <param name="templatePath">模板路径</param>
    static string GetTemplate(string templatePath)
    {
        if (File.Exists(templatePath))
        {
            return File.ReadAllText(templatePath);
        }
        else
        {
            Debug.LogError("Template file not found at: " + templatePath);
            return string.Empty;
        }
    }

    /// <summary>
    /// 转换首字母大小写
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="isUpper">是否转换成大写</param>
    /// <returns></returns>
    static string ConvertFirstChar(string content, bool isUpper = true)
    {
        if (string.IsNullOrEmpty(content)) return content;
        char firstChar = isUpper ? char.ToUpper(content[0]) : char.ToLower(content[0]);
        string restOfContent = content.Substring(1);
        return firstChar + restOfContent;
    }
}