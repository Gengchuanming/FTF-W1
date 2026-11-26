// The About page shows the app, its version and license, and the third-party notices rendered from the Markdown that
// ships inside the assembly (THIRD-PARTY-NOTICES.md).

using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Filee.App.Services;
using Filee.App.ViewModels;
using Filee.App.ViewModels.Pages;
using Filee.App.Views;
using Filee.App.Views.Pages;
using Filee.Core.Localization;
using Xunit;

namespace Filee.App.Tests;

public class AboutPageTests
{
    [AvaloniaTheory]
    [InlineData("en")]
    [InlineData("zh-CN")]
    public void About_page_shows_the_version_and_the_third_party_notices(string language)
    {
        TestServices.EnsureInitialized(language);
        using var vm = new MainWindowViewModel(AppHost.Services, AppHost.Get<ILocalizer>());
        vm.Navigate("about");
        var window = new MainWindow { DataContext = vm, Width = 1080, Height = 820 };
        window.Show();
        RenderTests.Pump();

        var page = window.GetVisualDescendants().OfType<AboutPage>().Single();
        var texts = page.GetVisualDescendants().OfType<TextBlock>()
            .Select(t => t.Text)
            .OfType<string>()
            .ToList();

        var loc = AppHost.Get<ILocalizer>();
        Assert.Contains(loc["about.license"], texts);
        Assert.Contains(texts, t => t.Contains(AppInfo.CurrentVersion, StringComparison.Ordinal));

        // The notices card renders the embedded Markdown.
        Assert.Contains(loc["about.notices"], texts);
        var model = Assert.IsType<AboutPageViewModel>(vm.CurrentPage);
        Assert.False(string.IsNullOrWhiteSpace(model.Notices), "the notices Markdown must load from the assembly");

        // Rows of the notices table prove the Markdown became real controls, not a wall of text.
        // MarkdownView fills SelectableTextBlock.Inlines (not .Text) and puts links in HyperlinkButtons.
        var rendered = page.GetVisualDescendants()
            .Select(v => v switch
            {
                SelectableTextBlock selectable => selectable.Inlines is { } inlines
                    ? string.Concat(inlines.OfType<Run>().Select(r => r.Text))
                    : selectable.Text,
                TextBlock block => block.Text,
                HyperlinkButton link => link.Content as string,
                _ => null,
            })
            .OfType<string>()
            .Where(t => t.Length > 0)
            .ToList();
        Assert.Contains(rendered, t => t.Contains("Avalonia UI", StringComparison.Ordinal));
        Assert.Contains(rendered, t => t.Contains("CommunityToolkit.Mvvm", StringComparison.Ordinal));

        // The page carries the notices and nothing else: no contact link of any kind.
        Assert.DoesNotContain(page.GetVisualDescendants().OfType<HyperlinkButton>(),
            b => b.NavigateUri?.Scheme is "mailto" or "msteams");

        RenderTests.Save(window, $"page-about-{language}.png");
        window.Close();
    }
}
