﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Kanji.Database.Dao;
using Kanji.Database.Entities;
using Kanji.Database.Models;
using Kanji.Interface.Models;
using Kanji.Common.Extensions;
using Kanji.Common.Helpers;
using Avalonia.Logging;
using Avalonia;
using Microsoft.Extensions.Logging;

namespace Kanji.Interface.Business
{
    public abstract class UserResourceSetManager<T>
    {
        #region Constants

        private static readonly string InfoFilePath = "info.xml";

        private static readonly string XmlNode_Info = "info";
        private static readonly string XmlNode_Author = "author";
        private static readonly string XmlNode_Date = "date";
        private static readonly string XmlNode_Name = "name";
        private static readonly string XmlNode_Description = "description";

        #endregion

        #region Constructors

        public UserResourceSetManager()
        {
            
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reads the metadata file of the set.
        /// </summary>
        /// <param name="directoryPath">
        /// Path to the base directory of the set.</param>
        /// <returns>Set info read from the file, or null if
        /// any error occured.</returns>
        public UserResourceSetInfo ReadInfo(string directoryPath)
        {
            try
            {
                string infoFilePath = Path.Combine(directoryPath, InfoFilePath);

                XDocument xdoc = XDocument.Load(infoFilePath);
                XElement xinfo = xdoc.Root;
                XElement xauthor = xinfo.Element(XmlNode_Author);
                XElement xdate = xinfo.Element(XmlNode_Date);
                XElement xname = xinfo.Element(XmlNode_Name);
                XElement xdescription = xinfo.Element(XmlNode_Description);

                UserResourceSetInfo info = new UserResourceSetInfo();
                info.Path = directoryPath;
                if (xauthor != null)
                {
                    info.Author = xauthor.Value.Trim();
                }
                if (xdate != null)
                {
                    DateTimeOffset dateValue = DateTimeOffset.MinValue;
                    DateTimeOffset.TryParseExact(xdate.Value.Trim(), "yyyy-MM-dd",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue);
                    info.Date = dateValue == DateTimeOffset.MinValue ? (DateTimeOffset?)null : dateValue;
                }
                if (xname != null)
                {
                    info.Name = xname.Value.Trim();
                }
                else
                {
                    throw new Exception("The set must have a name.");
                }
                if (xdescription != null)
                {
                    info.Description = xdescription.Value.Trim();
                }

                return info;
            }
            catch (Exception ex)
            {
                LogHelper.Factory.CreateLogger(GetType())
                    .LogError(ex,
                    "Error while loading the info of the set at \"{directory}\".",
                    directoryPath);

                return null;
            }
        }

        /// <summary>
        /// Attempts to read the data of the set located at the given path.
        /// </summary>
        /// <param name="directoryPath">Path to the set directory.</param>
        /// <returns>Data read. Returns a default value if an error occurs.</returns>
        public async Task<T> ReadData(string directoryPath)
        {
            try
            {
                return await DoReadData(directoryPath);
            }
            catch (Exception ex)
            {
                LogHelper.Factory.CreateLogger(GetType())
                    .LogError(ex,
                    "Error while loading the data of the set at \"{directory}\".",
                    directoryPath);

                return default;
            }
        }

        /// <summary>
        /// In a subclass, reads the data of the set.
        /// </summary>
        /// <param name="directoryPath">Path to the base directory of the set.</param>
        /// <returns>Set data read.</returns>
        protected abstract Task<T> DoReadData(string directoryPath);

        #endregion
    }
}
