// About: version, license, third-party notices.

using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using Filee.App.Services;
using Filee.Core.Localization;
using Filee.Core.Settings;

namespace Filee.App.ViewModels.Pages;

public sealed partial class AboutPageViewModel : ObservableObject
{
    public AboutPageViewModel(ILocalizer loc, UserDataStore store)
    {
        Version = loc.Format("about.version", AppInfo.CurrentVersion);
    }

    public string Version { get; }

    public string Notices { get; } = ReadNotices();

    private static string ReadNotices()
    {
        var uri = new Uri("avares://FTF/Assets/THIRD-PARTY-NOTICES.md");
        if (!AssetLoader.Exists(uri))
            return "";
        using var reader = new StreamReader(AssetLoader.Open(uri));
        return reader.ReadToEnd();
    }
}
