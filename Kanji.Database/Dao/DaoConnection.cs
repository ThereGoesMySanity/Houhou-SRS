using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using SQLite;
using Kanji.Common.Helpers;
using Kanji.Database.Helpers;
using System.Linq;
using System.IO;

namespace Kanji.Database.Dao
{
    public class DaoConnection
    {
        private static DaoConnection _instance;
        public static DaoConnection Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DaoConnection();
                }
                return _instance;
            }
        }

        private Dictionary<DaoConnectionEnum, SQLiteAsyncConnection> _connections = new Dictionary<DaoConnectionEnum, SQLiteAsyncConnection>();
        public SQLiteAsyncConnection this[DaoConnectionEnum endpoint]
        {
            get
            {
                if (!_connections.ContainsKey(endpoint))
                {
                    _connections[endpoint] = new SQLiteAsyncConnection(
                        ConnectionStringHelper.GetConnectionString(endpoint));
                }
                return _connections[endpoint];
            }
        }
    }
}
