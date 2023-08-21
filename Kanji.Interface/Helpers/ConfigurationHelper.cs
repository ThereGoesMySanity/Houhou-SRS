using System;
using System.IO;
using Config.Net;
using Kanji.Common.Helpers;

namespace Kanji.Interface.Helpers
{
    public partial class ConfigurationHelper
    {
        public static IConfigurationHelper Instance;
    }
    public interface IConfigurationHelper
    {
        /// <summary>
        /// Initializes the user content access.
        /// </summary>
        void InitializeConfiguration();
        
        Stream OpenDataFile(string path);

        /// <summary>
        /// Stores the path to the user content root directory path.
        /// </summary>
        string UserContentDirectoryPath { get; }

        /// <summary>
        /// Stores the path to the user RadicalSets root directory.
        /// </summary>
        string UserContentRadicalDirectoryPath { get; }

        /// <summary>
        /// Stores the path to the user SRSLevelSets root directory.
        /// </summary>
        string UserContentSrsLevelDirectoryPath { get; }

        /// <summary>
        /// Stores the path to the SRS Database file.
        /// </summary>
        string UserContentSrsDatabaseFilePath { get; }

        /// <summary>
        /// Stores the path to the dictionary database file in the common data directory path.
        /// </summary>
        string CommonDataDictionaryDatabaseFilePath { get; }
    }
    public abstract partial class ConfigurationHelper : IConfigurationHelper
    {
        public ConfigurationHelper()
        {
        }
        #region Constants

        /// <summary>
        /// Stores the path to the dictionary database from the working directory.
        /// </summary>
        public static readonly string DictionaryDatabaseFilePath = "KanjiDatabase.sqlite";

        /// <summary>
        /// Stores the path to the user content replicator directory.
        /// Subdirectories and files will be replicated to the user content directory.
        /// </summary>
        public static readonly string DataUserContentDirectoryPath = "UserContent";

        /// <summary>
        /// Stores the path to the default database file contained in the data user content directory.
        /// </summary>
        public readonly string DataUserContentDefaultDatabaseFilePath = Path.Combine(
            DataUserContentDirectoryPath, "SrsDatabase.sqlite");

        /// <summary>
        /// Stores the path to the common data directory.
        /// Used to store the database, because trying to access a database in the installation directory
        /// may not work, depending on the path (e.g. Program Files).
        /// </summary>
        public abstract string CommonDataDirectoryPath { get; }

        /// <summary>
        /// Stores the path to the dictionary database file in the common data directory path.
        /// </summary>
        public string CommonDataDictionaryDatabaseFilePath => Path.Combine(
            CommonDataDirectoryPath, "KanjiDatabase.sqlite");

        /// <summary>
        /// Stores the path to the user content root directory path.
        /// </summary>
        public string UserContentDirectoryPath { get; protected set; } = Path.Combine(
#if DEBUG
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Houhou (Debug)");
#else
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Houhou");
#endif

        /// <summary>
        /// Stores the path to the user RadicalSets root directory.
        /// </summary>
        public string UserContentRadicalDirectoryPath { get; protected set; }

        /// <summary>
        /// Stores the path to the user SRSLevelSets root directory.
        /// </summary>
        public string UserContentSrsLevelDirectoryPath { get; protected set; }

        /// <summary>
        /// Stores the path to the SRS Database file.
        /// </summary>
        public string UserContentSrsDatabaseFilePath { get; protected set; }

        #endregion

        #region Methods

        public void InitializeConfiguration()
        {
            Properties.UserSettings.Instance = new ConfigurationBuilder<Properties.IUserSettings>()
                .UseIniFile(Path.Combine(UserContentDirectoryPath, "UserSettings.ini"))
                .Build();
            // First of all, check the user directory path.
            CheckUserDirectoryPath();

            // Make sure user content directories exist.
            CreateDirectoryIfNotExist(UserContentDirectoryPath);
            CreateDirectoryIfNotExist(UserContentRadicalDirectoryPath);
            CreateDirectoryIfNotExist(UserContentSrsLevelDirectoryPath);
            CreateDirectoryIfNotExist(CommonDataDirectoryPath);

            // Make sure the dictionary database exists in the common data directory.
            FileHelper.CopyIfDifferent(OpenDataFile(DictionaryDatabaseFilePath), CommonDataDictionaryDatabaseFilePath);

            // Make sure the initial user config files exist.
            ReplicateInitialUserContent();

            // Make sure the user database file exists.
            if (!File.Exists(UserContentSrsDatabaseFilePath))
            {
                OpenDataFile(DataUserContentDefaultDatabaseFilePath).CopyTo(new FileStream(UserContentSrsDatabaseFilePath, FileMode.OpenOrCreate, FileAccess.Write));
            }
        }

        /// <summary>
        /// If the user directory path is empty, uses the default value.
        /// </summary>
        private void CheckUserDirectoryPath()
        {
            if (string.IsNullOrWhiteSpace(Properties.UserSettings.Instance.UserDirectoryPath) || Properties.UserSettings.Instance.UserDirectoryPath.StartsWith("["))
            {
                // If path is empty, or starts with "[": for some reason the path was not replaced during installation.
                Properties.UserSettings.Instance.UserDirectoryPath = UserContentDirectoryPath;
            }
            else
            {
                try
                {
                    CreateDirectoryIfNotExist(Properties.UserSettings.Instance.UserDirectoryPath);
                    UserContentDirectoryPath = Properties.UserSettings.Instance.UserDirectoryPath;
                }
                catch (Exception)
                {
                    // Cannot use this directory. Leave the default path.
                }
            }

            UserContentRadicalDirectoryPath = Path.Combine(UserContentDirectoryPath, "Radicals");
            UserContentSrsLevelDirectoryPath = Path.Combine(UserContentDirectoryPath, "SrsLevels");
            UserContentSrsDatabaseFilePath = Path.Combine(UserContentDirectoryPath, "SrsDatabase.sqlite");
        }

        public abstract string[] GetDataDirs(string path);
        public abstract string[] GetDataFiles(string path);
        public abstract Stream OpenDataFile(string path);

        /// <summary>
        /// Replicates the initial user content from the application data files
        /// to the user content directory if needed.
        /// </summary>
        protected void ReplicateInitialUserContent()
        {
            // Create all of the directories
            foreach (string dirPath in GetDataDirs(DataUserContentDirectoryPath))
            {
                string newPath = dirPath.Replace(DataUserContentDirectoryPath, UserContentDirectoryPath);
                CreateDirectoryIfNotExist(newPath);
            }

            // Copy all the files
            foreach (string filePath in GetDataFiles(DataUserContentDirectoryPath))
            {
                string newPath = filePath.Replace(DataUserContentDirectoryPath, UserContentDirectoryPath);

                if (!File.Exists(newPath))
                {
                    OpenDataFile(filePath).CopyTo(new FileStream(newPath, FileMode.Create, FileAccess.Write));
                }
            }
        }

        /// <summary>
        /// Creates a directory accessed by the given path if it
        /// does not already exist.
        /// </summary>
        /// <param name="directoryPath">Path to the directory
        /// to create.</param>
        protected static void CreateDirectoryIfNotExist(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }


        #endregion
    }
}
