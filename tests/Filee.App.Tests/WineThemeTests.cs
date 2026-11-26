// The wine default: the brand's accent pair, its dark ink/fill split, and the status colours it forced apart.

using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Filee.App.Services;
using Filee.Core.Settings;

namespace Filee.App.Tests;

public class WineThemeTests
{
    private static SolidColorBrush Brush(string key, ThemeVariant variant)
    {
        var window = new Window();
        Assert.True(window.TryFindResource(key, variant, out var value), $"Failed to resolve {key} for {variant}");
        return Assert.IsType<SolidColorBrush>(value);
    }

    [Fact]
    public void The_default_accent_is_the_wine_and_the_first_swatch_matches()
    {
        Assert.Equal("#A83838", SoftPalette.DefaultAccent);
        Assert.Equal(ThemeService.Swatches[0], SoftPalette.DefaultAccent);
        Assert.Equal("#A83838", new ThemeSettings().Accent);
    }

    [AvaloniaFact]
    public void The_wine_is_one_colour_in_light_and_a_pair_in_dark()
    {
        TestServices.EnsureInitialized("en");
        var themeService = AppHost.Get<ThemeService>();
        try
        {
            themeService.Apply(new ThemeSettings(), "en");

            // Light: the wine serves as both the ink and the fill.
            Assert.Equal(Color.Parse("#A83838"), Brush("AccentBrush", ThemeVariant.Light).Color);
            Assert.Equal(Color.Parse("#A83838"), Brush("AccentFillBrush", ThemeVariant.Light).Color);

            // Dark: the fill keeps the brand's deep tone, the ink is lightened from it, and white text sits on the fill.
            var darkFill = Brush("AccentFillBrush", ThemeVariant.Dark).Color;
            var darkInk = Brush("AccentBrush", ThemeVariant.Dark).Color;
            Assert.Equal(Color.Parse("#862C2C"), darkFill);
            Assert.True(SoftPalette.Contrast(darkInk, Color.Parse(SoftPalette.DarkSurface)) >= 6.0,
                $"dark ink {darkInk} must read on the dark surface");
            Assert.True(SoftPalette.Contrast(Colors.White, darkFill) >= 4.5,
                $"white text must read on the dark fill {darkFill}");
            Assert.Equal(Colors.White, Brush("OnAccentBrush", ThemeVariant.Dark).Color);
        }
        finally
        {
            themeService.Apply(new ThemeSettings(), "en");
        }
    }

    [AvaloniaFact]
    public void The_danger_no_longer_sits_on_the_accent()
    {
        TestServices.EnsureInitialized("en");
        var themeService = AppHost.Get<ThemeService>();
        try
        {
            themeService.Apply(new ThemeSettings(), "en");

            foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            {
                var accent = Brush("AccentBrush", variant).Color;
                var danger = Brush("DangerBrush", variant).Color;
                var fill = Brush("DangerFillBrush", variant).Color;

                // The two hues must be tellable apart: with the wine at 0°, a danger at the same hue is invisible.
                Assert.True(Math.Abs(Hue(accent) - Hue(danger)) >= 20,
                    $"{variant}: danger {danger} is too close to the accent {accent}");

                // The danger ink reads on the page; the fill carries white text.
                var page = Brush("PageBrush", variant).Color;
                Assert.True(SoftPalette.Contrast(danger, page) >= 4.5,
                    $"{variant}: danger ink {danger} must read on the page {page}");
                Assert.True(SoftPalette.Contrast(Colors.White, fill) >= 4.5,
                    $"{variant}: white must read on the danger fill {fill}");
            }
        }
        finally
        {
            themeService.Apply(new ThemeSettings(), "en");
        }
    }

    [AvaloniaFact]
    public void The_light_page_is_the_wine_tint_and_secondary_text_still_reads_on_it()
    {
        TestServices.EnsureInitialized("en");
        var themeService = AppHost.Get<ThemeService>();
        try
        {
            themeService.Apply(new ThemeSettings { Mode = ThemeMode.Light }, "en");
            var page = Brush("PageBrush", ThemeVariant.Light).Color;
            var alt = Brush("SurfaceAltBrush", ThemeVariant.Light).Color;
            var secondary = Brush("TextSecondaryBrush", ThemeVariant.Light).Color;

            Assert.Equal(Color.Parse("#F5EBEB"), page);
            Assert.True(SoftPalette.Contrast(secondary, alt) >= 4.5,
                $"secondary text {secondary} must read on the surface alt {alt}");
        }
        finally
        {
            themeService.Apply(new ThemeSettings(), "en");
        }
    }

    [AvaloniaFact]
    public void A_custom_accent_darkens_nothing_but_gets_a_readable_dark_ink()
    {
        TestServices.EnsureInitialized("en");
        var themeService = AppHost.Get<ThemeService>();
        try
        {
            // A grey so dark it is unreadable on the dark surface: the dark variant must lighten it into an ink.
            themeService.Apply(new ThemeSettings { Style = ThemeStyle.Soft, Accent = "#5F5E5A" }, "en");
            var darkInk = Brush("AccentBrush", ThemeVariant.Dark).Color;
            Assert.True(SoftPalette.Contrast(darkInk, Color.Parse(SoftPalette.DarkSurface)) >= 6.0,
                $"custom accent's dark ink {darkInk} must read on the dark surface");

            // The user's colour itself stays the fill and the light ink.
            Assert.Equal(Color.Parse("#5F5E5A"), Brush("AccentBrush", ThemeVariant.Light).Color);
            Assert.Equal(Color.Parse("#5F5E5A"), Brush("AccentFillBrush", ThemeVariant.Light).Color);
        }
        finally
        {
            themeService.Apply(new ThemeSettings(), "en");
        }
    }

    private static double Hue(Color c)
    {
        var (h, _, _) = MaterialYouPalette.RgbToHsl(c);
        return h * 360;
    }
}
