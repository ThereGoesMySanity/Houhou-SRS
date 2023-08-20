using System;
using System.IO;
using System.Linq;
using Android.Content;
using Kanji.Interface.Helpers;
using Kanji.Interface.Properties;

namespace Kanji.Android;

public class AndroidConfigurationHelper : ConfigurationHelper
{
    private readonly Context context;

    public override string CommonDataDirectoryPath { get; } = Path.Combine(
#if DEBUG
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Houhou SRS", "Debug");
#else
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Houhou SRS");
#endif

    public AndroidConfigurationHelper(Context context) : base()
    {
        this.context = context;
    }

    public override string[] GetDataDirs(string path) => context.Assets.List(path)
            .Where(f => context.Assets.List($"{path}/{f}").Length > 0)
            .Select(f => $"{path}/{f}")
            .SelectMany(f => context.Assets.List(f).Length == 0? Array.Empty<string>() : GetDataDirs(f).Append(f))
            .ToArray();

    public override string[] GetDataFiles(string path) => context.Assets.List(path)
            .Select(f => $"{path}/{f}")
            .SelectMany(f => context.Assets.List(f).Length == 0? new[]{f} : GetDataFiles(f))
            .ToArray();
    

    public override Stream OpenDataFile(string path)
    {
        return context.Assets.Open(path);
    }
}