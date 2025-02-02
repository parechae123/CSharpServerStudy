using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace CSharpServerStudy.Server.Handle
{
    internal class DBConnector
    {
        string dbPath;
        string dbPort;
        string dataTableName;
        string dbID;
        string dbPassword;
        public MySqlConnection conn;

        public string connectingString
        {
            get
            {
                return $"Server={dbPath},Port={dbPort};Database={dataTableName};User ID={dbID};Password={dbPassword}";
            }
        }
        public DBConnector(string dbIP,string port,string dbName,string userID,string password)
        {
            dbPath = dbIP;
            dbPort = port;
            dataTableName = dbName;
            dbID = userID;
            dbPassword = password;
        }

        public MySqlConnection DBConnect()
        {
            using (MySqlConnection conn = new MySqlConnection(connectingString))
            {
                conn.Open();
                Console.WriteLine("DB 연결 성공");
                Console.WriteLine(connectingString);
                this.conn = conn;
                return conn;
            }
        }
        public void DBDisConnect()
        {
            conn.Close();
        }

    }
}
