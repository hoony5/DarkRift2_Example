using System;
using System.Collections.Generic;
using System.Reflection;
using ClientServerModel;
using MySql.Data.MySqlClient;
using static LogicServer.MariaDBEx;

public static class AvatarDBLogic 
{
    private static FieldInfo[] avtFieldsInfo = new FieldInfo[0];
    private static FieldInfo[] dbFieldsInfo = new FieldInfo[0];
        
    public static readonly Dictionary<string, AvatarInfo> idToAvtInfoTable = new Dictionary<string, AvatarInfo>(0);
        
    public static bool TryLoad(string accountID, out AvatarInfo avt, out string log)
    {
        try
        {
            if (!idToAvtInfoTable.ContainsKey(accountID))
            {
                avt = new AvatarInfo();
                log = "There is no pk";
                return false;
            }

            avt = idToAvtInfoTable[accountID];
            log = "Load Success\n";
            return true;
        } 
        catch (Exception exception)
        {
            exception.Log();
            avt = new AvatarInfo();
                
            log = $"{exception.Message}\n{exception.StackTrace}";
            return false;
        }
    }   
    public static void InitAvtDB()
    {
        SetAvtTable();

        if (idToAvtInfoTable.Count == 0)
            return;
            
        Console.WriteLine("---------------Load Avatar DB---------------");
        foreach (var item in idToAvtInfoTable)
            Console.WriteLine($" :::: Loaded :: {item.Key} | {item.Value.pk} ::::\n");
        Console.WriteLine("---------------End---------------");
    }
    static void SetAvtTable()
    {
        try
        {
            InitFieldInfos(typeof(AvatarDBInfo), ref dbFieldsInfo);
            InitFieldInfos(typeof(AvatarInfo), ref avtFieldsInfo);

            string sql = $"select * from {AvatarDBInfo.AvatarTableName}";

            using (MySqlConnection conn = new MySqlConnection(MariaDBManager.ConnectionData))
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                    
                idToAvtInfoTable.Clear();
                var rowStrings = new string[0];
                var getString = string.Empty;
                    
                // 계정 DB 열 정보 가져오기
                var intArray = GetDBColumnIndices(reader, typeof(AvatarDBInfo), dbFieldsInfo);
                    
                // 모든 행 읽기
                while (reader.Read())
                {
                    var avatarInfo = new AvatarInfo();
                        
                    for (int i = 0; i < reader.FieldCount; i++)
                        getString = GetStringInDBTable(reader, i, ref getString);
                        
                    rowStrings = getString.Split(',');

                    for (int k = 0; k < avtFieldsInfo.Length; k++)
                        SetClass(ref avatarInfo, avtFieldsInfo[k], rowStrings[intArray[k]]);
                        
                        
                    var accountID = NetworkData.GetUserAccountID(rowStrings[0]);
                    AddAvtInfo(accountID, avatarInfo);
                        
                    getString = string.Empty;
                    Array.Clear(rowStrings, 0, rowStrings.Length);
                }
 
                reader.Close();
            }
        }
        catch (Exception exception)
        {
            exception.Log();
        }
    }

    public static void AddAvtInfo(string pk, AvatarInfo avatarInfo)
    {
        if (!idToAvtInfoTable.ContainsKey(pk))
            idToAvtInfoTable.Add(pk, avatarInfo);
        else
            idToAvtInfoTable[pk] = avatarInfo;
    }
    public static void RemoveAvtInfo(string pk)
    {
        if(idToAvtInfoTable.ContainsKey(pk))
            idToAvtInfoTable.Remove(pk);
    }
         
    // DB 업데이트용도
    static (string, string)[] SetDBColumnValuePairs(AvatarInfo avt)
    {    
        var infos = new List<(string, string)>(0);
        var avtDBFieldInfo = typeof(AvatarDBInfo);
            
        for (int i = 0; i < avtFieldsInfo.Length; i++)
        {
            Console.WriteLine($"| 입력 AvtInfo : Value {avtFieldsInfo[i].GetValue(avt)} - index [{i}] - type {avtFieldsInfo[i].FieldType} | 출력 DB Column : Value {dbFieldsInfo[i + 1].GetValue(avtDBFieldInfo)} |\n");
            infos.Add((dbFieldsInfo[i + 1].GetValue(avtDBFieldInfo).ToString(), avtFieldsInfo[i].GetValue(avt).ToString()));
        }

        return infos.ToArray();
    }
    public static (string, string)[] GetDBColumnValuePairs(AvatarInfo avt)
    {
        // (string : column name , string : column value)
        try
        {
            InitFieldInfos(typeof(AvatarInfo),ref avtFieldsInfo);
            InitFieldInfos(typeof(AvatarDBInfo), ref dbFieldsInfo);
                
            SetDBColumnValuePairs(avt);
            return SetDBColumnValuePairs(avt);
        }
        catch (Exception exception)
        {
            exception.Log();
            return new (string, string)[0];
        }
    }
}