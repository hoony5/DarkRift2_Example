using System;
using System.Reflection;
using MySql.Data.MySqlClient;

public static class MariaDBEx
{
    public static void InitFieldInfos(Type type, ref FieldInfo[] infos)
    {
        if (infos == null)
            infos = new FieldInfo[0];
            
        if(infos.Length == 0)
            infos = type.GetFields();
    }

    public static int[] GetDBColumnIndices(MySqlDataReader reader, Type type, FieldInfo[] fieldInfos)
    {
        var intArray = new int[fieldInfos.Length];
            
        for (int i = 1; i < intArray.Length; i++)
            intArray[i - 1] = reader.GetOrdinal(fieldInfos[i].GetValue(type).ToString());

        return intArray;
    }
    public static string GetStringInDBTable(MySqlDataReader reader, int columnIndex, ref string getString)
    {
        var str = string.Empty;
        if (IsString(reader,columnIndex))
            str = GetString(reader, columnIndex);

        if (IsFloat(reader, columnIndex))
            str = GetFloat(reader, columnIndex).ToString();
            
        if (IsInt(reader, columnIndex))
            str = GetInt(reader, columnIndex).ToString();
            
        getString += StringDBFormat(reader, columnIndex, str);

        return getString;
    }
    public static Type GetColumnType(MySqlDataReader reader, int columnIndex)
    {
        return reader.GetFieldType(columnIndex);
    }

    public static bool IsString(MySqlDataReader reader, int columnIndex)
    {
        return GetColumnType(reader, columnIndex) == typeof(string);
    }
    public static bool IsFloat(MySqlDataReader reader, int columnIndex)
    {
        return GetColumnType(reader, columnIndex) == typeof(float);
    }
    public static bool IsInt(MySqlDataReader reader, int columnIndex)
    {
        return GetColumnType(reader, columnIndex) == typeof(int);
    }
    public static string GetString(MySqlDataReader reader, int columnIndex)
    {
        return reader.GetString(columnIndex);
    }
    public static float GetFloat(MySqlDataReader reader, int columnIndex)
    {
        return reader.GetFloat(columnIndex);
    }
    public static int GetInt(MySqlDataReader reader, int columnIndex)
    {
        return reader.GetInt32(columnIndex);
    }

    public static string StringDBFormat(MySqlDataReader reader, int columnIndex, string str)
    {
        return columnIndex == reader.FieldCount - 1 ? str : $"{str},";
    }
    public static void SetClass<T>(ref T data, FieldInfo info, string value) where T : new()
    {
        if (info.FieldType == typeof(string))
            info.SetValue(data, value);
        if (info.FieldType == typeof(float))
            info.SetValue(data, float.Parse(value));
        if (info.FieldType == typeof(int))
            info.SetValue(data, int.Parse(value));
    }

}