using System;
using System.IO;
using System.Linq;
using Kanji.Common.Helpers;
using Kanji.Database.Helpers;
using Kanji.Interface.Helpers;
using Kanji.Interface.Properties;

namespace Kanji.Desktop;

public class DesktopConfigurationHelper : ConfigurationHelper
{
    /// <summary>
    /// Stores the path to the root application data directory,
    /// from the working directory.
    /// </summary>
    private readonly string DataRootPath = "Data";


    public override string CommonDataDirectoryPath { get; } = Path.Combine(
#if DEBUG
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "Houhou SRS", "Debug");
#else
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "Houhou SRS");
#endif

    public override string[] GetDataDirs(string path) => Directory.GetDirectories(Path.Combine(DataRootPath, path), "*",
                SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(DataRootPath, f)).ToArray();
    public override string[] GetDataFiles(string path) => Directory.GetFiles(Path.Combine(DataRootPath, path), "*",
                SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(DataRootPath, f)).ToArray();

    public override Stream OpenDataFile(string path)
    {
        return File.OpenRead(Path.Combine(DataRootPath, path));
    }
}