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
        public static DaoConnection Instance;
        public DaoConnection(string kanjiDb, string srsDb)
        {
            _paths = new Dictionary<DaoConnectionEnum, string>()
            {
                [DaoConnectionEnum.KanjiDatabase] = kanjiDb,
                [DaoConnectionEnum.SrsDatabase] = srsDb,
            };
        }

        private Dictionary<DaoConnectionEnum, SQLiteAsyncConnection> _connections = new Dictionary<DaoConnectionEnum, SQLiteAsyncConnection>();
        private Dictionary<DaoConnectionEnum, string> _paths;

        public SQLiteAsyncConnection this[DaoConnectionEnum endpoint]
        {
            get
            {
                if (!_connections.ContainsKey(endpoint))
                {
                    _connections[endpoint] = new SQLiteAsyncConnection(_paths[endpoint]);
                }
                return _connections[endpoint];
            }
        }
    }
}
