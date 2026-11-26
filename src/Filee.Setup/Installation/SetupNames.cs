// Names Setup shares with the app and Windows. FTF has its own identity: a GUID of its own, so it never collides
// with (or "updates") an installed copy of the original Filee.

namespace Filee.Setup.Installation;

public static class SetupNames
{
    /// <summary>FTF's own Apps &amp; Features identity, under the uninstall key "{id}_is1".</summary>
    public const string AppGuid = "69C7B565-EA5D-48C1-9636-0FC178068A8E";

    public const string UninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\{" + AppGuid + "}_is1";

    public const string MachineEnvironmentKey = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
    public const string UserEnvironmentKey = "Environment";

    public const string AppName = "FTF";
    public const string Publisher = "Geng";
    public const string AppExe = "FTF.exe";

    /// <summary>The uninstaller next to FTF.exe (the app counts itself as installed when it exists).</summary>
    public const string UninstallerExe = "uninstall.exe";

    /// <summary>The list of installed files next to FTF.exe: what the uninstaller removes and an update replaces.</summary>
    public const string InstallRecordFile = "uninstall.json";

    /// <summary>The "ftf" command (cli\ftf.cmd), the folder Setup puts on PATH.</summary>
    public const string CliFolder = "cli";

    /// <summary>Filee 1.x's Inno Setup uninstaller files, removed when Setup takes over a 1.x installation.</summary>
    public static readonly IReadOnlyList<string> InnoUninstallerFiles = ["unins000.exe", "unins000.dat", "unins000.msg"];

    /// <summary>Filee 1.1 and earlier: Velopack installed it per user and registered it here (HKCU).</summary>
    public const string VelopackUninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\Filee";

    /// <summary>Registry values Inno Setup wrote, read when taking over a 1.x installation.</summary>
    public const string InnoTasksValue = "Inno Setup: Selected Tasks";
    public const string InnoAppPathValue = "Inno Setup: App Path";
    public const string InnoVersionValue = "Inno Setup: Setup Version";
    public const string InnoLanguageValue = "Inno Setup: Language";

    /// <summary>Start menu and desktop shortcut name.</summary>
    public const string ShortcutName = "FTF.lnk";

    /// <summary>Registry values Setup adds to the uninstall key.</summary>
    public const string TasksValue = "FTF Setup: Tasks";
    public const string LanguageValue = "FTF Setup: Language";
    public const string ScopeValue = "FTF Setup: Scope";

    /// <summary>Windows 10 1809: the oldest Windows FTF runs on.</summary>
    public const int MinimumBuild = 17763;

    /// <summary>Where Microsoft offers the Visual C++ Redistributable (x64).</summary>
    public const string VcRedistUrl = "https://aka.ms/vs/17/release/vc_redist.x64.exe";
}
