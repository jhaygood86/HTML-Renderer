using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/MediaQueryTests.cs.
/// HTML-Renderer's CSS engine port added real <c>@media</c> evaluation: <see cref="TheArtOfDev.HtmlRenderer.Core.MediaQueryMatcher"/>
/// evaluates a rule's enclosing <c>MediaList</c> chain (parsed by the vendored engine's real grammar,
/// including "not"/"only" modifiers and comma-separated media lists - see <c>Medium.IsInverse</c> and a
/// <c>MediaList</c>'s multiple comma-separated <c>Medium</c> entries) against a
/// <see cref="TheArtOfDev.HtmlRenderer.Core.MediaQueryContext"/> built from <see cref="TheArtOfDev.HtmlRenderer.Adapters.RAdapter.DefaultMediaType"/>.
/// The old standalone <c>CssData.GetCssBlock(name, media)</c>/<c>ContainsCssBlock(name, media)</c> bucket
/// API this file originally used no longer exists at all - <c>CssData</c> is now purely a rule index
/// queried per-box during the real cascade (<c>CssData.GetStyleRules</c>), so these tests instead drive
/// the whole pipeline via <see cref="LayoutHarness"/> and read the resolved value off the laid-out box.
/// <see cref="MockAdapter.MediaType"/> selects which device the layout runs under (mirroring how a real
/// PdfSharpAdapter/WinFormsAdapter reports its own <c>DefaultMediaType</c>).
/// Because "not"/"only" and comma-lists are now real, the three cases previously <c>[Ignore]</c>d as "not
/// yet spec compliant" (the whitespace-only splitter mis-tokenizing them) now genuinely pass against the
/// real grammar and are un-ignored below.
/// </summary>
[TestClass]
public sealed class MediaQueryTests
{
    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);

    private static CssBox LayoutAndFindTag(string css, string bodyHtml, string tag, string mediaType)
    {
        var adapter = new MockAdapter { MediaType = mediaType };
        var html = $"<!DOCTYPE html><html><head><style>{css}</style></head><body>{bodyHtml}</body></html>";
        var (root, _) = LayoutHarness.Layout(html, adapter: adapter);
        var box = LayoutHarness.Descendants(root).FirstOrDefault(b => b.HtmlTag != null && b.HtmlTag.Name == tag);
        Assert.IsNotNull(box);
        return box!;
    }

    // ── basic single media type + nested rules ──────────────────────────────────────────────────────────

    [TestMethod]
    public void AtMedia_Print_NestedRuleAppliesUnderPrintMedia()
    {
        var box = LayoutAndFindTag("@media print { p { color: red; } }", "<p>text</p>", "p", "print");
        Assert.AreEqual(Red, box.ActualColor);
    }

    [TestMethod]
    public void AtMedia_Print_DoesNotApplyUnderScreenMedia()
    {
        // the rule is scoped to the "print" media only - it must not leak into "screen".
        var box = LayoutAndFindTag("@media print { p { color: red; } }", "<p>text</p>", "p", "screen");
        Assert.AreNotEqual(Red, box.ActualColor);
    }

    [TestMethod]
    public void AtMedia_Screen_NestedRuleAppliesUnderScreenMedia()
    {
        var box = LayoutAndFindTag("@media screen { p { color: red; } }", "<p>text</p>", "p", "screen");
        Assert.AreEqual(Red, box.ActualColor);
    }

    [TestMethod]
    public void AtMedia_All_NestedRuleAppliesUnderAllMedia()
    {
        var box = LayoutAndFindTag("@media all { p { color: red; } }", "<p>text</p>", "p", "screen");
        Assert.AreEqual(Red, box.ActualColor);
    }

    [TestMethod]
    public void AtMedia_Print_MultipleNestedRulesAreParsed()
    {
        const string css = "@media print { p { color: red; } div { font-size: 12pt; } }";
        const string body = "<p>text</p><div>text</div>";

        var pBox = LayoutAndFindTag(css, body, "p", "print");
        Assert.AreEqual(Red, pBox.ActualColor);

        var divBox = LayoutAndFindTag(css, body, "div", "print");
        Assert.AreEqual("12pt", divBox.FontSize);
    }

    // ── "not" / "only" modifiers - now real ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void AtMedia_NotPrint_ExcludesPrintButAppliesElsewhere()
    {
        const string css = "@media not print { p { color: red; } }";

        var underPrint = LayoutAndFindTag(css, "<p>text</p>", "p", "print");
        Assert.AreNotEqual(Red, underPrint.ActualColor);

        var underScreen = LayoutAndFindTag(css, "<p>text</p>", "p", "screen");
        Assert.AreEqual(Red, underScreen.ActualColor);
    }

    [TestMethod]
    public void AtMedia_OnlyPrint_AppliesOnlyUnderPrint()
    {
        const string css = "@media only print { p { color: red; } }";

        var underPrint = LayoutAndFindTag(css, "<p>text</p>", "p", "print");
        Assert.AreEqual(Red, underPrint.ActualColor);

        var underScreen = LayoutAndFindTag(css, "<p>text</p>", "p", "screen");
        Assert.AreNotEqual(Red, underScreen.ActualColor);
    }

    // ── comma-separated media list - now real ───────────────────────────────────────────────────────────

    [TestMethod]
    public void AtMedia_PrintCommaScreen_AppliesUnderBoth()
    {
        const string css = "@media print, screen { p { color: red; } }";

        var underPrint = LayoutAndFindTag(css, "<p>text</p>", "p", "print");
        Assert.AreEqual(Red, underPrint.ActualColor);

        var underScreen = LayoutAndFindTag(css, "<p>text</p>", "p", "screen");
        Assert.AreEqual(Red, underScreen.ActualColor);
    }
}
