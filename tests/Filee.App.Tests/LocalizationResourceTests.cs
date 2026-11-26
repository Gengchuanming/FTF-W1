using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.VisualTree;
using Filee.App.Controls;
using Filee.App.Services;
using Filee.App.ViewModels;
using Filee.App.Views;
using Filee.Core.Localization;

namespace Filee.App.Tests;

public class LocalizationResourceTests
{
    [AvaloniaFact]
    public void Arabic_font_chain_draws_trimming_ellipsis_and_matches_arabic_letters()
    {
        TestServices.EnsureInitialized("zh-CN");
        try
        {
            var family = FontFamily.Parse(ThemeService.FontFamilies("zh-CN"));
            // Avalonia takes the trimming symbol from the primary typeface rather than applying character fallback.
            Assert.NotEqual((ushort)0, new Typeface(family).GlyphTypeface.CharacterToGlyphMap.GetGlyph(0x2026));
            Assert.True(FontManager.Current.TryMatchCharacter(0x0645, FontStyle.Normal, FontWeight.Normal,
                FontStretch.Normal, family, System.Globalization.CultureInfo.GetCultureInfo("zh-CN"), out var arabic));
            Assert.NotEqual((ushort)0, arabic.GlyphTypeface.CharacterToGlyphMap.GetGlyph(0x0645));
        }
        finally { TestServices.EnsureInitialized("en"); }
    }

    [AvaloniaFact]
    public void Strings_are_available_as_application_resources()
    {
        TestServices.EnsureInitialized("zh-CN");
        Assert.True(Application.Current!.TryGetResource("home.title", null, out var value));
        Assert.Equal("首页", value);
    }

    [AvaloniaFact]
    public void DynamicResource_in_xaml_resolves_and_follows_language_changes()
    {
        TestServices.EnsureInitialized("en");
        var text = new TextBlock();
        text.Bind(TextBlock.TextProperty, text.GetResourceObservable("home.title"));
        var toggle = new ToggleSwitch();
        var window = new Window { Content = new StackPanel { Children = { text, toggle } } };
        window.Show();
        Assert.Equal("Home", text.Text);

        TestServices.EnsureInitialized("zh-CN");
        Assert.Equal("首页", text.Text);
        Assert.Equal("开", toggle.OnContent);
        Assert.Equal("关", toggle.OffContent);
        window.Close();
    }
}
