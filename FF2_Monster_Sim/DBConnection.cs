using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace FF2_Monster_Sim
{
    public class DBConnection
    {
        private DBConnection()
        {
            //
        }

        public string DatabaseName = String.Empty;
        public string Password = String.Empty;
        public MySqlConnection Connection { get; private set; } = null;

        private static DBConnection _instance = null;
        public static DBConnection Instance()
        {
            if (_instance == null)
                _instance = new DBConnection();
            return _instance;
        }

        public bool IsConnected()
        {
            if (Connection == null)
            {
                if (String.IsNullOrEmpty(DatabaseName))
                {
                    Debug.WriteLine("Null or empty database name supplied");
                    return false;
                }
                if (String.IsNullOrEmpty(Password))
                {
                    Debug.WriteLine("Null or empty password supplied");
                    return false;
                }

                string connstring = string.Format("Server=localhost; database={0}; UID=root; password=" + Password, DatabaseName);
                Connection = new MySqlConnection(connstring);
                Connection.Open();
            }

            return true;
        }

        public void Close()
        {
            Connection.Close();
        }
    }
}