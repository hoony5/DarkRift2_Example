using System;
using System.Reflection;
using ClientServerModel;
using MySql.Data.MySqlClient;
using static LogicServer.MariaDBEx;
using static LogicServer.NetworkData;
using Math = ClientServerModel.Math;
    public class PrivateAccountDatas
    {
        public string pk = string.Empty;
        public string id = string.Empty;
        public string password = string.Empty;
        public string nickName = string.Empty;
        public float last_x = 0;
        public float last_y = 0;
        public float last_z = 0;

        public PrivateAccountDatas SetPk(string pk)
        {
            this.pk = pk;
            return this;
        }

        public PrivateAccountDatas SetID(string id)
        {
            this.id = id;
            return this;
        }

        public PrivateAccountDatas SetPasswd(string passwd)
        {
            password = passwd;
            return this;
        }

        public PrivateAccountDatas SetNickName(string name)
        {
            nickName = name;
            return this;
        }

        public PrivateAccountDatas SetLastPos(float x, float y, float z)
        {
            last_x = x;
            last_y = y;
            last_z = z;
            return this;
        }
    }
    public static class AccountDBLogic
    {
        private static FieldInfo[] dbFieldsInfo = new FieldInfo[0];
        private static FieldInfo[] dbPrivateInfo = new FieldInfo[0];
        
        static void SetAccountTable()
        {
            try
            {
                InitFieldInfos(typeof(AccountDBInfo), ref dbFieldsInfo);
                InitFieldInfos(typeof(PrivateAccountDatas), ref dbPrivateInfo);
                    
                idToAccountTable.Clear();
                pkToAccountTable.Clear();

                string sql = $"select * from {AccountDBInfo.AccountTableName}";

                using (MySqlConnection conn = new MySqlConnection(MariaDBManager.ConnectionData))
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    var rowStrings = new string[0];
                    var getString = string.Empty;
                    
                    // 계정 DB 열 정보 가져오기
                    var accountIDIndex = reader.GetOrdinal(AccountDBInfo.ColumnAccountID);
                    var intArray = GetDBColumnIndices(reader, typeof(AccountDBInfo), dbFieldsInfo);
                    
                    // 모든 행 읽기
                    while (reader.Read())
                    {
                        var accountData = new PrivateAccountDatas();
                        
                        for (int i = 0; i < reader.FieldCount; i++)
                            getString = GetStringInDBTable(reader, i, ref getString);
                        
                        rowStrings = getString.Split(',');
                        
                        for (int k = 0; k < dbPrivateInfo.Length; k++)
                            SetClass(ref accountData, dbPrivateInfo[k], rowStrings[intArray[k]]);
                        
                        SaveUserData(rowStrings[accountIDIndex], accountData);
                        
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

        public static void InitAccountDB()
        {
            SetAccountTable();

            if (idToAccountTable.Count <= 0) return;

            Console.WriteLine("---------------Load Account DB---------------");
            foreach (var item in idToAccountTable)
                Console.WriteLine(
                    $" :::: Loaded :: {item.Key} | {item.Value.id} | {item.Value.password} | {item.Value.pk}  | {item.Value.id} Exsist ?  {MariaDBManager.ExistDB(AccountDBInfo.AccountTableName, AccountDBInfo.ColumnAccountID, item.Value.id)}::::\n");
            Console.WriteLine("---------------End---------------");
        }

        public static bool TryLogIn(string accountId, string accountPw, out string log)
        {
            log = "Success";
            try
            {
                var existID = ExsistDB(AccountDBInfo.ColumnAccountID, accountId);
                var existPw = ExsistDB(AccountDBInfo.ColumnAccountPassword, accountPw);
                
                if (!existID)
                {
                    LogicServerPlugin.Debug("There is no the ID !!");
                    log += " | There is no the ID !! | ";
                    return false;
                }

                if (existPw) return true;
                
                LogicServerPlugin.Debug("The password is wrong !!");
                log += " | The password is wrong !! | ";
                return false;

            }
            catch (Exception exception)
            {
                exception.Log();
                return false;
            }
        }

        public static bool ExsistDB(string columnName, string comparedValue)
        {
            return MariaDBManager.ExistDB(AccountDBInfo.AccountTableName, columnName, comparedValue);
        }

        public static Math.ServerVector3 GetLastPos(string accountID)
        {
            if (!idToAccountTable.ContainsKey(accountID))
            {
                LogicServerPlugin.Debug("There is no the ID");
                return new Math.ServerVector3(0, 0, 0);
            }


            return new Math.ServerVector3(
                idToAccountTable[accountID].last_x,
                idToAccountTable[accountID].last_y,
                idToAccountTable[accountID].last_z);
        }
    }