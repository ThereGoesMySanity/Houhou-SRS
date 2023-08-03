using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Kanji.Common.Helpers;
using Kanji.Database.Dao;

namespace Kanji.Database.Helpers
{
    public static class ConnectionStringHelper
    {
        public static readonly string KanjiDatabaseConnectionName = "KanjiDatabase";
        //public static readonly string SrsDatabaseConnectionName = "SrsDatabase";
        public static string SrsDatabaseConnectionString;


        public static void SetSrsDatabasePath(string path)
        {
            SrsDatabaseConnectionString = path;
        }

        /// <summary>
        /// Gets the connection string associated with the given endpoint.
        /// </summary>
        /// <param name="endpoint">Target endpoint.</param>
        /// <returns>Connection string associated with the given endpoint.</returns>
        internal static string GetConnectionString(DaoConnectionEnum endpoint)
        {
            switch (endpoint)
            {
                case DaoConnectionEnum.KanjiDatabase:
                    return Path.Combine(
                        (string)AppDomain.CurrentDomain.GetData("DataDirectory"), "KanjiDatabase.sqlite");
                case DaoConnectionEnum.SrsDatabase:
                    return ConnectionStringHelper.SrsDatabaseConnectionString;
                default:
                    throw new ArgumentException(
                        string.Format("Unknown connection: \"{0}\".", endpoint));
            }
        }
    }
}
