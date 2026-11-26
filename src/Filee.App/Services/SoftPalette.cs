// The resources behind Filee's look that are computed rather than written in Styles/Soft.axaml: the accent colour
// with its shades, the corner radii, the UI font order and the reading direction. ThemeService applies them with the user's choices; the
// installer (src/Filee.Setup links this file) applies the defaults, so Setup looks like the app.

using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Filee.App.Services;

internal static class SoftPalette
{
    /// <summary>The default accent colour (the first swatch of the Theme page).</summary>
    public const string DefaultAccent = "#A83838";

    /// <summary>
    /// The default accent's dark variant. A dark wine reads well as a fill (white text on it is 8.71:1) but is
    /// unreadable as text on the dark page (1.98:1), so the dark variant splits the accent into a fill and a lighter
    /// ink derived from it. See <see cref="DarkInk"/> and <see cref="DarkFillFor"/>.
    /// </summary>
    public const string DefaultAccentDark = "#862C2C";

    /// <summary>The dark page and surface the contrast rules are measured against.</summary>
    public const string DarkPage = "#1B1B19";
    public const string DarkSurface = "#262624";

    private static readonly Color DarkSurfaceColor = Color.Parse(DarkSurface);

    /// <summary>The default corner radius (ThemeSettings.CornerRadius).</summary>
    public const double DefaultCornerRadius = 16;

    /// <summary>Embedded Noto fonts (see build/fetch-fonts.ps1). Missing files fall back to system fonts.</summary>
    private const string Latin = "fonts:FTF#Noto Sans";
    private const string Arabic = "fonts:FTF#Noto Sans Arabic";
    private const string Devanagari = "fonts:FTF#Noto Sans Devanagari";
    private const string Thai = "fonts:FTF#Noto Sans Thai";

    /// <summary>Each CJK region's embedded font and its system fonts (Windows, then macOS).</summary>
    private static readonly (string Embedded, string System) Japanese =
        ("fonts:FTF#Noto Sans JP", "Yu Gothic UI, Meiryo, Hiragino Sans");
    private static readonly (string Embedded, string System) Korean =
        ("fonts:FTF#Noto Sans KR", "Malgun Gothic, Apple SD Gothic Neo");
    private static readonly (string Embedded, string System) Chinese =
        ("fonts:FTF#Noto Sans SC", "Microsoft YaHei, PingFang SC");

    /// <summary>
    /// Traditional Chinese has no embedded font (two more Noto CJK files would add 11 MB): Windows and macOS always carry
    /// these. Listed before the embedded Noto Sans SC, whose Han shapes differ, so zh-TW text takes Traditional shapes.
    /// </summary>
    private const string TraditionalChineseSystem = "Microsoft JhengHei UI, Microsoft JhengHei, PingFang TC";

    /// <summary>All resource keys written by ApplyAccent that must be cleared from top level when per-variant dictionaries are used.</summary>
    public static readonly string[] AccentResourceKeys =
    [
        "AccentColor",
        "AccentBrush",
        "AccentFillBrush",
        "AccentEdgeBrush",
        "AccentSoftBrush",
        "AccentFaintBrush",
        "AccentStrongBrush",
        "OnAccentBrush",
        "SystemAccentColor",
        "SystemAccentColorLight1",
        "SystemAccentColorLight2",
        "SystemAccentColorLight3",
        "SystemAccentColorDark1",
        "SystemAccentColorDark2",
        "SystemAccentColorDark3",
    ];

    /// <summary>Removes top-level accent keys so ThemeDictionaries take effect without being shadowed.</summary>
    public static void ClearTopLevelAccent(IResourceDictionary res)
    {
        foreach (var key in AccentResourceKeys)
            res.Remove(key);
    }

    /// <summary>
    /// Accent roles for one variant. The accent splits in two: <c>AccentBrush</c> is the ink (text, icons, focus
    /// rings, outlines) and <c>AccentFillBrush</c> is what carries white text (primary buttons, checks, toggles,
    /// the highlighted slice). On a light page the accent reads as both; on the dark page the same wine is only
    /// 1.98:1 as ink, so the ink is lightened until it reads while the fill keeps the brand's deep tone.
    /// </summary>
    /// <param name="res">Dictionary to write into (a theme dictionary, or the application's resources).</param>
    /// <param name="accent">The variant's accent colour: the fill.</param>
    /// <param name="isDark">True when the roles are for the dark variant.</param>
    public static void ApplyAccent(IResourceDictionary res, Color accent, bool isDark)
    {
        var fill = isDark ? DarkFillFor(accent) : accent;
        var ink = isDark ? DarkInk(fill) : accent;
        var onAccent = Luminance(fill) > 0.6 ? Color.Parse("#1F1E1D") : Colors.White;

        res["AccentColor"] = fill;
        res["AccentBrush"] = new SolidColorBrush(ink);
        res["AccentFillBrush"] = new SolidColorBrush(fill);
        // On dark the deep wine fill is only 1.74:1 against the card, so a filled control needs a lit outline to
        // read as a shape at all (WCAG 1.4.11 asks 3:1 for a component's boundary). On light the fill already
        // stands 6.39:1 off the surface, and the edge is the fill itself: no visible line.
        res["AccentEdgeBrush"] = new SolidColorBrush(isDark ? Mix(fill, ink, 0.5) : fill);
        res["AccentSoftBrush"] = new SolidColorBrush(isDark ? Mix(DarkSurfaceColor, ink, 0.18) : Mix(Colors.White, fill, 0.22));
        res["AccentFaintBrush"] = new SolidColorBrush(isDark ? Mix(DarkSurfaceColor, ink, 0.10) : Mix(Colors.White, fill, 0.14));
        res["AccentStrongBrush"] = new SolidColorBrush(isDark ? Shade(ink, 0.15) : Shade(fill, -0.25));
        res["OnAccentBrush"] = new SolidColorBrush(onAccent);
        res["SystemAccentColor"] = fill;
        res["SystemAccentColorLight1"] = Shade(fill, 0.15);
        res["SystemAccentColorLight2"] = Shade(fill, 0.30);
        res["SystemAccentColorLight3"] = Shade(fill, 0.45);
        res["SystemAccentColorDark1"] = Shade(fill, -0.15);
        res["SystemAccentColorDark2"] = Shade(fill, -0.30);
        res["SystemAccentColorDark3"] = Shade(fill, -0.45);
    }

    /// <summary>Accent colour + derived shades for the light variant.</summary>
    public static void ApplyAccent(IResourceDictionary res, Color accent) => ApplyAccent(res, accent, isDark: false);

    /// <summary>
    /// The ink form of <paramref name="accent"/> on the dark surface: the accent itself when it already reads,
    /// otherwise lightened in HSL (hue and saturation kept, so a wine stays a wine) until it clears 6:1 — a
    /// comfortable margin, not the bare 4.5, because this ink also draws icons and focus rings.
    /// </summary>
    public static Color DarkInk(Color accent)
    {
        if (Contrast(accent, DarkSurfaceColor) >= 6.0)
            return accent;
        var (h, s, _) = ToHsl(accent);
        for (var step = 1; step <= 100; step++)
        {
            var candidate = FromHsl(h, s, Math.Min(0.95, step * 0.01));
            if (Contrast(candidate, DarkSurfaceColor) >= 6.0)
                return candidate;
        }
        return Colors.White;
    }

    /// <summary>sRGB → HSL. Kept local: Setup links this file without the rest of the app's services.</summary>
    private static (double H, double S, double L) ToHsl(Color c)
    {
        double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;
        double h = 0, s = 0;
        var l = (max + min) / 2;
        if (delta > 1e-9)
        {
            s = l < 0.5 ? delta / (max + min) : delta / (2 - max - min);
            if (Math.Abs(max - r) < 1e-9)
                h = ((g - b) / delta + (g < b ? 6 : 0)) / 6;
            else if (Math.Abs(max - g) < 1e-9)
                h = ((b - r) / delta + 2) / 6;
            else
                h = ((r - g) / delta + 4) / 6;
        }
        return (h, s, l);
    }

    /// <summary>HSL → sRGB, the inverse of <see cref="ToHsl"/>.</summary>
    private static Color FromHsl(double h, double s, double l)
    {
        static double Channel(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2.0) return q;
            if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
            return p;
        }

        h = (h % 1.0 + 1.0) % 1.0;
        s = Math.Clamp(s, 0.0, 1.0);
        l = Math.Clamp(l, 0.0, 1.0);
        if (s < 1e-9)
        {
            var grey = (byte)Math.Clamp(Math.Round(l * 255), 0, 255);
            return Color.FromRgb(grey, grey, grey);
        }
        var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
        var p = 2 * l - q;
        byte R(double t) => (byte)Math.Clamp(Math.Round(Channel(p, q, t) * 255), 0, 255);
        return Color.FromRgb(R(h + 1.0 / 3.0), R(h), R(h - 1.0 / 3.0));
    }

    /// <summary>
    /// The accent's fill for the dark variant. The shipped default's dark fill is its deeper wine (#862C2C), the
    /// tone the brand pairs with #A83838; any other accent fills with itself.
    /// </summary>
    public static Color DarkFillFor(Color accent) =>
        accent == Color.Parse(DefaultAccent) ? Color.Parse(DefaultAccentDark) : accent;

    /// <summary>WCAG 2.1 contrast ratio between two opaque colours (1..21).</summary>
    public static double Contrast(Color a, Color b)
    {
        var x = RelativeLuminance(a);
        var y = RelativeLuminance(b);
        return (Math.Max(x, y) + 0.05) / (Math.Min(x, y) + 0.05);
    }

    /// <summary>Mixes <paramref name="from"/> towards <paramref name="to"/> by <paramref name="amount"/> (0..1).</summary>
    private static Color Mix(Color from, Color to, double amount)
    {
        byte Blend(byte a, byte b) => (byte)Math.Round(a + (b - a) * Math.Clamp(amount, 0, 1));
        return Color.FromArgb(255, Blend(from.R, to.R), Blend(from.G, to.G), Blend(from.B, to.B));
    }

    /// <summary>WCAG 2.1 relative luminance (sRGB, gamma corrected).</summary>
    public static double RelativeLuminance(Color c)
    {
        static double Channel(byte v)
        {
            var s = v / 255.0;
            return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
        }
        return 0.2126 * Channel(c.R) + 0.7152 * Channel(c.G) + 0.0722 * Channel(c.B);
    }

    /// <summary>
    /// Applies variant-specific monochrome accent resources into a theme dictionary so selection
    /// maintains high contrast in both light (dark charcoal) and dark (silvery white) modes.
    /// </summary>
    public static void ApplyMonochromeAccent(IResourceDictionary dict, bool isDark)
    {
        var accent = isDark ? Color.Parse("#F1EFE8") : Color.Parse("#1F1E1D");
        var onAccent = isDark ? Color.Parse("#1F1E1D") : Colors.White;
        dict["AccentColor"] = accent;
        dict["AccentBrush"] = new SolidColorBrush(accent);
        dict["AccentFillBrush"] = new SolidColorBrush(accent);
        dict["AccentEdgeBrush"] = new SolidColorBrush(accent);
        dict["AccentSoftBrush"] = new SolidColorBrush(accent, isDark ? 0.18 : 0.12);
        dict["AccentFaintBrush"] = new SolidColorBrush(accent, isDark ? 0.08 : 0.05);
        dict["AccentStrongBrush"] = new SolidColorBrush(accent);
        dict["OnAccentBrush"] = new SolidColorBrush(onAccent);
        dict["SystemAccentColor"] = accent;
        dict["SystemAccentColorLight1"] = Shade(accent, 0.15);
        dict["SystemAccentColorLight2"] = Shade(accent, 0.30);
        dict["SystemAccentColorLight3"] = Shade(accent, 0.45);
        dict["SystemAccentColorDark1"] = Shade(accent, -0.15);
        dict["SystemAccentColorDark2"] = Shade(accent, -0.30);
        dict["SystemAccentColorDark3"] = Shade(accent, -0.45);
    }

    /// <summary>"Softness": one radius drives every control.</summary>
    public static void ApplyCornerRadius(IResourceDictionary res, double radius)
    {
        var r = Math.Clamp(radius, 0, 28);
        res["SoftRadius"] = new CornerRadius(r);
        res["SoftRadiusSmall"] = new CornerRadius(Math.Round(r * 0.55));
        res["SoftRadiusLarge"] = new CornerRadius(Math.Round(r * 1.35));
        res["ControlCornerRadius"] = new CornerRadius(Math.Round(r * 0.55));
        res["OverlayCornerRadius"] = new CornerRadius(r);
        res["SoftRadiusPill"] = new CornerRadius(Math.Round(r * 2.5));
    }

    /// <summary>
    /// The UI font chain. Han characters look different in Japanese, Korean and Chinese fonts, so the UI language's
    /// CJK fonts come first, embedded and system ones alike; other languages keep Korean first.
    /// </summary>
    public static string FontFamilies(string language)
    {
        (string Embedded, string System)[] cjk =
            language.StartsWith("ja", StringComparison.OrdinalIgnoreCase) ? [Japanese, Korean, Chinese]
            : language.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ? [Chinese, Korean, Japanese]
            : [Korean, Chinese, Japanese];
        var traditional = AppLanguages.IsTraditionalChinese(language) ? $"{TraditionalChineseSystem}, " : "";
        // Noto Sans comes first: the first face also draws the trimming ellipsis, which Noto Sans Arabic lacks.
        return $"{Latin}, {Arabic}, {Devanagari}, {Thai}, {traditional}{string.Join(", ", cjk.Select(f => f.Embedded))}, Segoe UI, Nirmala UI, Leelawadee UI, "
            + $"{string.Join(", ", cjk.Select(f => f.System))}, Kohinoor Devanagari, Geeza Pro, Thonburi, sans-serif";
    }

    /// <summary>
    /// The font chain from Windows' own UI fonts only (no embedded fonts), with the UI language's CJK font first: what
    /// the installer uses. Every Windows 10 1809+ has these, with bold faces.
    /// </summary>
    public static string SystemFontFamilies(string language)
    {
        (string Embedded, string System)[] cjk =
            language.StartsWith("ja", StringComparison.OrdinalIgnoreCase) ? [Japanese, Korean, Chinese]
            : language.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ? [Chinese, Korean, Japanese]
            : [Korean, Chinese, Japanese];
        var traditional = AppLanguages.IsTraditionalChinese(language) ? $"{TraditionalChineseSystem}, " : "";
        return $"Segoe UI, Nirmala UI, Leelawadee UI, {traditional}{string.Join(", ", cjk.Select(f => f.System))}, sans-serif";
    }

    /// <summary>The resource Soft.axaml's Window style takes <see cref="Visual.FlowDirection"/> from.</summary>
    public const string FlowDirectionKey = "AppFlowDirection";

    /// <summary>"→" between a name and what it becomes, "←" in a right-to-left language (arrows don't mirror).</summary>
    public const string ForwardArrowKey = "AppForwardArrow";

    /// <summary>
    /// Arabic reads right to left: every window mirrors its layout (Soft.axaml's Window style). Controls that draw a
    /// picture or a circle (the donut, the crop canvas, animated images) keep left to right themselves.
    /// </summary>
    public static void ApplyFlowDirection(IResourceDictionary res, string language)
    {
        var rightToLeft = IsRightToLeft(language);
        res[FlowDirectionKey] = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        res[ForwardArrowKey] = rightToLeft ? "←" : "→";
    }

    /// <summary>True for a language written right to left.</summary>
    public static bool IsRightToLeft(string language)
    {
        try
        {
            return CultureInfo.GetCultureInfo(language).TextInfo.IsRightToLeft;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }

    /// <summary>The UI font for <paramref name="language"/>.</summary>
    public static FontFamily FontFamilyFor(string language) => FontFamily.Parse(FontFamilies(language));

    /// <summary>Lightens (amount &gt; 0) or darkens (amount &lt; 0) a colour.</summary>
    public static Color Shade(Color c, double amount)
    {
        byte Mix(byte v) => amount >= 0
            ? (byte)Math.Round(v + (255 - v) * amount)
            : (byte)Math.Round(v * (1 + amount));
        return Color.FromArgb(c.A, Mix(c.R), Mix(c.G), Mix(c.B));
    }

    /// <summary>Relative luminance 0..1 (sRGB approximation).</summary>
    public static double Luminance(Color c) => (0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B) / 255.0;
}
