// Installs, updates and uninstalls against a registry in memory, a fake Windows and scratch folders: nothing here
// touches HKLM, HKCU, PATH, the Start menu or a real Filee.

extern alias app;
using Filee.Setup.Installation;
using Filee.Setup.Platform;
using CommandLine = app::Filee.App.Services.CommandLine;

namespace Filee.Setup.Tests;

public class InstallerTests
{
    private const string Key = SetupNames.UninstallKey;

    [Fact]
    public void A_fresh_install_puts_everything_in_place()
    {
        using var bench = new SetupBench();
        var result = bench.Install();
        Assert.True(result.Succeeded, result.Error?.ToString());
        var folder = bench.InstallFolder;

        foreach (var file in new[] { "FTF.exe", "Filee.dll", @"cli\ftf.cmd", @"engines\rhwp\rhwp.exe", "uninstall.exe", "uninstall.json" })
            Assert.True(File.Exists(Path.Combine(folder, file)), file);
        Assert.Empty(Directory.EnumerateDirectories(folder, FileSwap.WorkFolderPrefix + "*"));
        var record = InstallRecord.Read(folder)!;
        Assert.Equal("2.0.0", record.Version);
        Assert.Contains("uninstall.exe", record.Files);
        Assert.Contains(@"engines\rhwp\rhwp.exe", record.Files);

        // Apps & Features: Inno Setup's key, with everything Windows shows.
        var registry = bench.Registry;
        string? Value(string name) => registry.Get(Hive.LocalMachine, Key, name)?.Text;
        Assert.Equal("FTF", Value("DisplayName"));
        Assert.Equal("2.0.0", Value("DisplayVersion"));
        Assert.Equal("Geng", Value("Publisher"));
        Assert.Equal(Path.Combine(folder, "FTF.exe"), Value("DisplayIcon"));
        Assert.Equal(folder + "\\", Value("InstallLocation"));
        Assert.Equal($"\"{folder}\\uninstall.exe\"", Value("UninstallString"));
        Assert.Equal($"\"{folder}\\uninstall.exe\" /VERYSILENT /SUPPRESSMSGBOXES /NORESTART", Value("QuietUninstallString"));
        Assert.Equal(1, registry.Get(Hive.LocalMachine, Key, "NoModify")?.Value);
        Assert.True((int)registry.Get(Hive.LocalMachine, Key, "EstimatedSize")!.Value > 0);
        Assert.Equal(2, registry.Get(Hive.LocalMachine, Key, "VersionMajor")?.Value);
        Assert.Equal("contextmenu,startup,cli", Value(SetupNames.TasksValue));

        // Start menu, PATH (REG_EXPAND_SZ), no desktop shortcut by default.
        Assert.True(File.Exists(Path.Combine(bench.Paths.CommonStartMenu, "FTF.lnk")));
        Assert.False(File.Exists(Path.Combine(bench.Paths.CommonDesktop, "FTF.lnk")));
        var path = registry.Get(Hive.LocalMachine, SetupNames.MachineEnvironmentKey, "Path")!;
        Assert.True(path.Expandable);
        Assert.True(PathVariable.Contains(path.Text, Path.Combine(folder, "cli")));
        Assert.Contains("broadcast", bench.System.Calls);

        // Filee starts with the choices; it applies them for the signed-in user.
        Assert.Equal(Path.Combine(folder, "FTF.exe"), result.Launch!.Executable);
        Assert.Equal("--install-engines=pandoc --start-with-windows=on --context-menu=on", result.Launch.Arguments);
    }






    [Fact]
    public void Updating_our_own_install_replaces_exactly_its_files()
    {
        using var bench = new SetupBench(version: "2.0.0", app: (temp, _) => temp.Write(@"app\Removed.dll", "only in 2.0.0"));
        Assert.True(bench.Install().Succeeded);
        var folder = bench.InstallFolder;
        // Something of the user's in the folder stays.
        File.WriteAllText(Path.Combine(folder, "notes.txt"), "mine");
        File.Delete(Path.Combine(bench.AppFolder, "Removed.dll"));
        File.WriteAllText(Path.Combine(bench.AppFolder, "FTF.exe"), "Filee 2.1");
        bench.MakeSetup("2.1.0");

        var previous = PreviousInstall.Find(bench.Registry, InstallScope.Machine)!;
        Assert.False(previous.ByInno);
        Assert.NotNull(previous.Record);
        var result = bench.Install(bench.Choices(previous.CurrentTasks(bench.Registry, bench.System, bench.Paths)) with { Engines = [] }, previous);
        Assert.True(result.Succeeded, result.Error?.ToString());
        Assert.False(File.Exists(Path.Combine(folder, "Removed.dll")));
        Assert.True(File.Exists(Path.Combine(folder, "notes.txt")));
        Assert.Equal("Filee 2.1", File.ReadAllText(Path.Combine(folder, "FTF.exe")));
        Assert.Equal("2.1.0", InstallRecord.Read(folder)!.Version);
        Assert.Equal("2.1.0", bench.Registry.Get(Hive.LocalMachine, Key, "DisplayVersion")?.Text);
    }

    [Fact]
    public void Unticking_on_an_update_removes_the_desktop_icon_and_the_path_entry()
    {
        using var bench = new SetupBench();
        Assert.True(bench.Install(bench.Choices(SetupTasks.Defaults with { DesktopIcon = true })).Succeeded);
        var desktop = Path.Combine(bench.Paths.CommonDesktop, "FTF.lnk");
        Assert.True(File.Exists(desktop));
        var previous = PreviousInstall.Find(bench.Registry, InstallScope.Machine)!;
        var tasks = previous.CurrentTasks(bench.Registry, bench.System, bench.Paths) with { DesktopIcon = false, Cli = false };
        Assert.True(bench.Install(bench.Choices(tasks), previous).Succeeded);
        Assert.False(File.Exists(desktop));
        Assert.False(PathVariable.IsOnPath(bench.Registry, InstallScope.Machine, Path.Combine(bench.InstallFolder, "cli")));
    }

    [Fact]
    public void A_failure_halfway_puts_the_previous_version_back()
    {
        using var bench = new SetupBench();
        Assert.True(bench.Install().Succeeded);
        var folder = bench.InstallFolder;
        var before = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
            .ToDictionary(f => f, File.ReadAllText);
        var entryBefore = bench.Registry.ValueNames(Hive.LocalMachine, Key)
            .ToDictionary(n => n, n => bench.Registry.Get(Hive.LocalMachine, Key, n)!.Value);
        File.WriteAllText(Path.Combine(bench.AppFolder, "FTF.exe"), "Filee 3");
        bench.MakeSetup("3.0.0");
        File.Delete(Path.Combine(bench.Paths.CommonStartMenu, "FTF.lnk"));

        // The Start menu shortcut can't be written: everything after the file swap fails.
        bench.System.OnCreateShortcut = _ => throw new IOException("Access denied (test)");
        var previous = PreviousInstall.Find(bench.Registry, InstallScope.Machine)!;
        var result = bench.Install(previous: previous);
        Assert.False(result.Succeeded);
        Assert.True(result.RolledBack);
        Assert.Null(result.Launch);

        var after = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).ToDictionary(f => f, File.ReadAllText);
        Assert.Equal(before, after);
        Assert.Equal("2.0.0", bench.Registry.Get(Hive.LocalMachine, Key, "DisplayVersion")?.Text);
        Assert.Equal(entryBefore.Keys.Order(), bench.Registry.ValueNames(Hive.LocalMachine, Key).Order());
    }

    [Fact]
    public void A_damaged_payload_changes_nothing()
    {
        using var bench = new SetupBench();
        long offset;
        using (var payload = Filee.Setup.Payload.SetupPayload.Open(bench.SetupExe)!)
            offset = payload.Offset;
        var bytes = File.ReadAllBytes(bench.SetupExe);
        bytes[offset + 3] ^= 0x55;
        File.WriteAllBytes(bench.SetupExe, bytes);

        var result = bench.Install();
        Assert.False(result.Succeeded);
        Assert.IsType<InvalidDataException>(result.Error);
        Assert.False(result.RolledBack);
        Assert.False(Directory.Exists(bench.InstallFolder));
        Assert.False(bench.Registry.KeyExists(Hive.LocalMachine, Key));
        Assert.Empty(bench.System.Calls);
    }

    [Fact]
    public void No_icons_and_portable()
    {
        using var bench = new SetupBench();
        Assert.True(bench.Install(bench.Choices() with { NoIcons = true }).Succeeded);
        Assert.False(File.Exists(Path.Combine(bench.Paths.CommonStartMenu, "FTF.lnk")));

        using var portable = new SetupBench();
        var folder = portable.Temp.Sub("Portable Filee");
        var result = portable.Install(portable.Choices() with { Folder = folder, Portable = true }, previous: null);
        Assert.True(result.Succeeded);
        Assert.Null(result.Launch);
        Assert.True(File.Exists(Path.Combine(folder, "FTF.exe")));
        // No uninstaller: the app counts itself as portable (UpdateService.IsInstalled).
        Assert.False(File.Exists(Path.Combine(folder, "uninstall.exe")));
        Assert.False(portable.Registry.KeyExists(Hive.LocalMachine, Key));
        Assert.Empty(portable.System.Calls);
    }

    [Fact]
    public void A_per_user_install_stays_in_the_users_part()
    {
        using var bench = new SetupBench();
        var folder = bench.Paths.DefaultInstallFolder(InstallScope.User);
        var result = bench.Install(bench.Choices() with { Folder = folder, Scope = InstallScope.User }, previous: null);
        Assert.True(result.Succeeded);
        Assert.True(bench.Registry.KeyExists(Hive.CurrentUser, Key));
        Assert.False(bench.Registry.KeyExists(Hive.LocalMachine, Key));
        Assert.True(PathVariable.IsOnPath(bench.Registry, InstallScope.User, Path.Combine(folder, "cli")));
        Assert.True(File.Exists(Path.Combine(bench.Paths.UserStartMenu, "FTF.lnk")));
        Assert.Equal(InstallScope.User, InstallRecord.Read(folder)!.Scope);
        Assert.NotNull(PreviousInstall.Find(bench.Registry, InstallScope.User));
    }

    [Fact]
    public void Filee_1_1_from_velopack_is_removed_but_its_engines_stay()
    {
        using var bench = new SetupBench();
        var root = VelopackInstall.Root(bench.Paths);
        bench.Temp.Write(Path.Combine(Path.GetRelativePath(bench.Temp.Path, root), "Update.exe"));
        bench.Temp.Write(Path.Combine(Path.GetRelativePath(bench.Temp.Path, root), @"current\FTF.exe"));
        bench.Temp.Write(Path.Combine(Path.GetRelativePath(bench.Temp.Path, root), @"packages\Filee-1.1.0-full.nupkg"));
        var engine = bench.Temp.Write(Path.Combine(Path.GetRelativePath(bench.Temp.Path, root), @"engines\pandoc\.filee-component"));
        bench.Registry.SetString(Hive.CurrentUser, SetupNames.VelopackUninstallKey, "DisplayName", "Filee");
        Assert.True(VelopackInstall.Exists(bench.Paths));

        Assert.True(bench.Install().Succeeded);
        Assert.False(File.Exists(Path.Combine(root, "Update.exe")));
        Assert.False(Directory.Exists(Path.Combine(root, "current")));
        Assert.False(Directory.Exists(Path.Combine(root, "packages")));
        Assert.True(File.Exists(engine));
        Assert.False(bench.Registry.KeyExists(Hive.CurrentUser, SetupNames.VelopackUninstallKey));
    }

    [Theory]
    [InlineData("en")]
                public void What_setup_hands_to_filee_is_what_filee_reads(string language)
    {
        var fresh = new InstallChoices(@"C:\Program Files\Filee", InstallScope.Machine, SetupTasks.Defaults with { Startup = false },
            ["pandoc", "ffmpeg"], language, LanguagePicked: true);
        var options = CommandLine.Parse(Installer.LaunchArguments(fresh, update: false).Split(' '));
        Assert.Equal(["pandoc", "ffmpeg"], options.InstallEngines);
        Assert.False(options.StartWithWindows);
        Assert.True(options.ContextMenu);
        Assert.Equal(language, options.Language);

        // Setup's language only goes to Filee when the user picked it; else Filee keeps following Windows.
        Assert.Null(CommandLine.Parse(Installer.LaunchArguments(fresh with { LanguagePicked = false }, update: false).Split(' ')).Language);
        // Nothing picked is an answer too: Filee then doesn't ask on first run.
        Assert.Empty(CommandLine.Parse(Installer.LaunchArguments(fresh with { Engines = [] }, update: false).Split(' ')).InstallEngines!);
        var updated = CommandLine.Parse(Installer.LaunchArguments(fresh, update: true).Split(' '));
        Assert.True(updated.Background);
        var shown = CommandLine.Parse(Installer.LaunchArguments(fresh, update: true, shown: true).Split(' '));
        Assert.False(shown.Background);
        Assert.Equal("--background", Installer.LaunchArguments(fresh, update: true, updatedFrom: "1.0 \"--quit"));
    }
}
