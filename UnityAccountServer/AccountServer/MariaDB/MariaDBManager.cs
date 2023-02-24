using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using DarkRift.Server;
using ClientServerModel;
using MySql.Data.MySqlClient;
using UnityClientServer.MariaDB;
using Math = System.Math;

public static class MariaDBManager
{
    private const float threshold = 0.001f;
    private static StringBuilder sb = new StringBuilder();
    public static string ConnectionData { get; private set; }

    private static string CreateConnectionData()
    {
        var onLocal = $"Server={MariaDBData.TestDBServerIPAdress};Database={MariaDBData.TestDBName};Uid ={MariaDBData.TestDBID};Pwd={MariaDBData.TestDBPassword};";

        var onServer = $"Server={MariaDBData.DBServerIPAdress};Database={MariaDBData.DBName};Uid ={MariaDBData.DBUserID};Pwd={MariaDBData.DBPassword};";

        return MariaDBData.DebugOn ? onLocal : onServer;
    }
    
    private static void Init()
    {
        ConnectionData = CreateConnectionData();
    }
    public static bool ConnectMariaDB()
    {
        Init();

        try
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionData))
            {
                conn.Open();
            }
            return true;
        }
        catch (Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(
                $"{MariaDBData.DBServerIPAdress} / {MariaDBData.DBName} / {MariaDBData.DBUserID} / {MariaDBData.DBPassword} | MariaDB Connection Failed!!");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(exception);
            return false;
        }
    }

    public static string[] SelectRow(string dataTableName, string standardColumn, string standardValue, ServerMessageReceivedEventArgs e)
    {
        try
        {
            string sql = $"select * from {dataTableName} where {standardColumn} ='{standardValue}'";
            sb.Clear();

            using (MySqlConnection conn = new MySqlConnection(ConnectionData))
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read() && !reader.IsClosed)
                {
                    for (int i = 0; i < reader.VisibleFieldCount; i++)
                    {
                        Type type = reader.GetFieldType(i);

                        if (type == typeof(string))
                        {
                            sb.Append(i == reader.VisibleFieldCount - 1
                                ? $"{reader.GetFieldValue<string>(i)}"
                                : $"{reader.GetFieldValue<string>(i)},");
                        }

                        if (type == typeof(float))
                        {
                            sb.Append(i == reader.VisibleFieldCount - 1
                                ? reader.GetFieldValue<float>(i).ToString()
                                : $"{reader.GetFieldValue<float>(i).ToString()},");
                        }

                        if (type != typeof(int)) continue;
                        sb.Append(i == reader.VisibleFieldCount - 1
                            ? reader.GetFieldValue<int>(i).ToString()
                            : $"{reader.GetFieldValue<int>(i).ToString()},");
                    }
                }
                reader.Close();
                return sb.ToString().Split(',');
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    public static bool ExistDB(string dataTableName, string columnName, string input)
    {
        try
        {
            string sql = $"select * from {dataTableName}";

            using (MySqlConnection conn = new MySqlConnection(ConnectionData))
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                int index = reader.GetOrdinal(columnName);
                Type columnDataType = reader.GetFieldType(index);

                while (reader.Read())
                {
                    if (columnDataType == typeof(string))
                    {
                        if (reader.GetString(index) == input)
                            return true;
                    }
                    else if (columnDataType == typeof(float))
                    {
                        if (Math.Abs(reader.GetFloat(index) - float.Parse(input)) < threshold)
                            return true;
                    }
                    else if (columnDataType == typeof(int))
                    {
                        if (reader.GetInt32(index) == int.Parse(input))
                            return true;
                    }    
                }
                reader.Close();
            }
            return false;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    public static void InsertDB(string dataTableName, params (string, string)[] CNameCValuePairs)
    {
        try
        {
            string columnsNames = string.Empty;
            string columnsValues = string.Empty;
                
            for (int i = 0; i < CNameCValuePairs.Length; i++)
            {
                (string name, string value) current = CNameCValuePairs[i];
                bool isLastIndex = i == CNameCValuePairs.Length - 1;
                    
                columnsNames += isLastIndex ? current.name : $"{current.name},";
                columnsValues += isLastIndex ? $"'{current.value}'" : $"'{current.value}',";
            }

            string sql = $"Insert Into {dataTableName} ({columnsNames}) values ({columnsValues})";
            
            //string sql = "Insert Into sampledatatable (ACCOUNT_ID) values ('abcdf234234423')";

            using (MySqlConnection conn = new MySqlConnection(ConnectionData))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
    public static void UpdateDB(string dataTableName, string whereColumnName, string whereColumnValue, string updateColumn, string updateValue)
    {
        try
        {
            string sql = $"Update {dataTableName} Set {updateColumn} ='{updateValue}' where {whereColumnName} ='{whereColumnValue}'";

            using (MySqlConnection conn = new MySqlConnection(ConnectionData))
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception exception)
        { 
            Console.WriteLine(exception);
        }
    }
        
    public static void UpdateMultipleDB(string dataTableName, string whereColumnName, string whereColumnValue, params (string ,string)[] updateColumnValuePairs)
    {
        try
        {
            sb.Clear();

            for (int i = 0; i < updateColumnValuePairs.Length; i++)
            {
                (string column, string value) current = updateColumnValuePairs[i];
                bool isLastIndex = updateColumnValuePairs.Length - 1 == i;
                sb.Append(isLastIndex ? $"{current.column} ='{current.value}'" :$"{current.column} ='{current.value}',");
            }
            
            string sql = $"Update {dataTableName} Set {sb}  where {whereColumnName} = '{whereColumnValue}'";
            
            using (MySqlConnection conn = new MySqlConnection(ConnectionData))
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception exception)
        {
            exception.Log();
            throw;
        }
    }

    public static void DeleteRow(string dataTableName, string whereColumnName, string whereColumnValue)
    {
        try
        {
            string sql = $"Delete From {dataTableName} where {whereColumnName} = '{whereColumnValue}'";
            AlarmConnectionDB(sql);

            using (MySqlConnection conn = new MySqlConnection(ConnectionData))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}