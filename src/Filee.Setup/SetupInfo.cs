using System.Reflection;

namespace Filee.Setup;

/// <summary>Facts about this Setup program.</summary>
public static class SetupInfo
{
    /// <summary>The version this Setup program was built as (the payload says which FTF it installs).</summary>
    public static string Version =>
        typeof(SetupInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "0.0.0";
}
