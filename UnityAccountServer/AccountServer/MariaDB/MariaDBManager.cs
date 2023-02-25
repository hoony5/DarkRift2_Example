using MySql.Data.MySqlClient;
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

    private static StringBuilder ReadValue(MySqlDataReader reader)
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

        return sb;
    }
    public static string[] SelectRow(string dataTableName, string targetColumn, string targetValue)
    {
        try
        {
            string sql = $"select * from {dataTableName} where {targetColumn} ='{targetValue}'";
            sb.Clear();

            using (MySqlConnection conn = new MySqlConnection(ConnectionData))
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read() && !reader.IsClosed)
                {
                    sb = ReadValue(reader);
                }

                reader.Close();
                return sb.ToString().Split(',');
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            return new string[1] { "NULL" };
        }
    }

    public static bool ExistDB(string dataTableName, string column, string input)
    {
        try
        {
            string sql = $"select * from {dataTableName}";

            using (MySqlConnection conn = new MySqlConnection(ConnectionData))
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                int index = reader.GetOrdinal(column);
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
            return false;
        }
    }

    public static bool InsertDB(string dataTableName, params (string column, string value)[] insert)
    {
        try
        {
            string columns = string.Empty;
            string values = string.Empty;
                
            for (int i = 0; i < insert.Length; i++)
            {
                (string name, string value) current = insert[i];
                bool isLastIndex = i == insert.Length - 1;
                    
                columns += isLastIndex ? current.name : $"{current.name},";
                values += isLastIndex ? $"'{current.value}'" : $"'{current.value}',";
            }

            string sql = $"Insert Into {dataTableName} ({columns}) values ({values})";

            using MySqlConnection conn = new MySqlConnection(ConnectionData);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            return false;
        }
    }
    
    public static bool UpdateDB(string dataTableName, string targetColumn, string targetValue, string updateColumn, string updateValue)
    {
        try
        {
            string sql = $"Update {dataTableName} Set {updateColumn} ='{updateValue}' where {targetColumn} ='{targetValue}'";

            using MySqlConnection conn = new MySqlConnection(ConnectionData);
            conn.Open();

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception exception)
        { 
            Console.WriteLine(exception);
            return false;
        }
    }
        
    // target column is originality of the value we want to update
    // update column is the column we want to update
    // update value is the new value
    public static bool UpdateMultipleDB(string dataTableName, string targetColumn, string targetValue, params (string column ,string value)[] update)
    {
        try
        {
            sb.Clear();

            for (int i = 0; i < update.Length; i++)
            {
                (string column, string value) current = update[i];
                bool isLastIndex = update.Length - 1 == i;
                sb.Append(isLastIndex ? $"{current.column} ='{current.value}'" :$"{current.column} ='{current.value}',");
            }
            
            string sql = $"Update {dataTableName} Set {sb}  where {targetColumn} = '{targetValue}'";

            using MySqlConnection conn = new MySqlConnection(ConnectionData);
            conn.Open();

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            return false;
        }
    }

    public static bool DeleteRow(string dataTableName, string targetColumn, string targetValue)
    {
        try
        {
            string sql = $"Delete From {dataTableName} where {targetColumn} = '{targetValue}'";

            using MySqlConnection conn = new MySqlConnection(ConnectionData);
            
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            return false;
        }
    }
}