// Facts about this build: version and whether it is a published (installed) copy. Replaces the parts of the former
// UpdateService that the app still needs after update checking was removed.

using System.Reflection;

namespace Filee.App.Services;

public static class AppInfo
{
    public static string CurrentVersion =>
        typeof(AppInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0]
        ?? typeof(AppInfo).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";

    /// <summary>True for published builds (the installer is published with <c>-p:FileeRelease=true</c>).</summary>
    public static bool IsReleaseBuild =>
        typeof(AppInfo).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Any(a => a.Key == "FileeRelease" && a.Value == "true");

    /// <summary>True when an uninstaller sits next to the program, i.e. this copy was installed.</summary>
    public static bool IsInstalled => IsInstalledIn(AppContext.BaseDirectory);

    internal static bool IsInstalledIn(string folder) =>
        File.Exists(Path.Combine(folder, "uninstall.exe")) || File.Exists(Path.Combine(folder, "unins000.exe"));
}
