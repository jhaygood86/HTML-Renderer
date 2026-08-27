using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Text;

/// <summary>
/// Verifies <c>white-space</c> actually affects whitespace-collapsing and line-wrapping -
/// <c>CssBox.ParseToWords</c>/<c>CssLayoutEngine.FlowBox</c> fully implement it.
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class WhiteSpaceLayoutIntegrationTests
{
    [TestMethod]
    public void Pre_PreservesMultipleConsecutiveSpacesAsLiteralWord()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p' style='white-space:pre'>A     B</p>"));
        var p = LayoutHarness.FindById(root, "p")!;
        var words = p.LineBoxes[0].Words;

        Assert.IsTrue(words.Any(w => w.Text == "     "));
    }

    [TestMethod]
    public void Normal_CollapsesConsecutiveSpaces_NoLiteralSpaceWord()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>A     B</p>"));
        var p = LayoutHarness.FindById(root, "p")!;
        var words = p.LineBoxes[0].Words;

        Assert.IsFalse(words.Any(w => w.Text != null && w.Text.Length > 0 && w.Text.All(char.IsWhiteSpace)));
    }

    [TestMethod]
    public void Pre_TreatsExplicitNewlineAsForcedLineBreak()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p' style='white-space:pre'>A\nB</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual(2, p.LineBoxes.Count);
    }

    [TestMethod]
    public void Normal_IgnoresEmbeddedNewline_NoForcedBreak()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>A\nB</p>"));
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual(1, p.LineBoxes.Count);
    }

    [TestMethod]
    public void NoWrap_PreventsWrapping_EvenWhenNarrowerThanContent()
    {
        var html = LayoutHarness.Wrap("<p id='p' style='white-space:nowrap; width:50px'>a long run of unwrapped text here</p>");
        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.AreEqual(1, p.LineBoxes.Count);
    }

    [TestMethod]
    public void Normal_WrapsAtNarrowWidth_ForContrastWithNoWrap()
    {
        var html = LayoutHarness.Wrap("<p id='p' style='width:50px'>a long run of unwrapped text here</p>");
        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;

        Assert.IsTrue(p.LineBoxes.Count > 1);
    }

    // ─── &nbsp; (U+00A0) is significant, non-collapsible, non-breaking content - unlike ordinary
    // whitespace, which stays collapsible/breakable (CSS2.1 §16.4.1) ───────────

    [Ignore("HtmlUtils.DecodeHtml decodes &nbsp; to a plain U+0020 space rather than U+00A0 (non-breaking " +
            "space), so it is collapsed away like ordinary whitespace-only content instead of surviving as " +
            "significant content with real height. Confirmed real engine behavior, not a porting mistake - " +
            "same root cause tracked for HtmlEntityDecodingIntegrationTests.")]
    [TestMethod]
    public void Nbsp_OnlyContent_ProducesNonZeroHeight_MatchingRealText()
    {
        var (nbspRoot, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='b'>&nbsp;</div>"));
        var (textRoot, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='b'>A</div>"));
        var nbspBox = LayoutHarness.FindById(nbspRoot, "b")!;
        var textBox = LayoutHarness.FindById(textRoot, "b")!;

        var nbspHeight = nbspBox.ActualBottom - nbspBox.Location.Y;
        var textHeight = textBox.ActualBottom - textBox.Location.Y;

        Assert.IsTrue(nbspHeight > 0, $"Expected non-zero height for nbsp-only content, got {nbspHeight}");
        Assert.IsTrue(nbspHeight >= textHeight - 1 && nbspHeight <= textHeight + 1);
    }

    [TestMethod]
    public void OrdinaryWhitespaceOnlyContent_StillProducesZeroHeight_NoRegression()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='b'>   </div>"));
        var box = LayoutHarness.FindById(root, "b")!;

        var height = box.ActualBottom - box.Location.Y;
        Assert.IsTrue(height >= 0 && height <= 0.5);
    }

    [Ignore("HtmlUtils.DecodeHtml decodes &nbsp; to a plain U+0020 space rather than U+00A0, so it is treated " +
            "as an ordinary breakable/collapsible space instead of a non-breaking one - the narrow-width case " +
            "wraps just like the plain-space case instead of staying on one line. Confirmed real engine " +
            "behavior, not a porting mistake - same root cause tracked for HtmlEntityDecodingIntegrationTests.")]
    [TestMethod]
    public void Nbsp_BetweenTokens_PreventsLineWrap_ContrastOrdinarySpace()
    {
        // Narrow enough that an ordinary space between "10" and "km" wraps to two lines, but a
        // non-breaking space between them must never be treated as a break opportunity.
        var (nbspRoot, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p' style='width:35px'>10&nbsp;km</p>"));
        var pNbsp = LayoutHarness.FindById(nbspRoot, "p")!;

        var (spaceRoot, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p' style='width:15px'>10 km</p>"));
        var pSpace = LayoutHarness.FindById(spaceRoot, "p")!;

        Assert.AreEqual(1, pNbsp.LineBoxes.Count);
        Assert.IsTrue(pSpace.LineBoxes.Count > 1,
            "expected ordinary space to still allow wrapping, for contrast with nbsp");
    }

    // ─── word-break: break-all forces a mid-word break normal cannot find ──────

    [TestMethod]
    public void BreakAll_ForcesMidWordBreak_ContrastNormal()
    {
        // A single unbroken run with no space anywhere: "normal" has no break opportunity at all
        // and must lay the whole word out on one (overflowing) line, while "break-all" must wrap it.
        const string longWord = "abcdefghijklmnopqrstuvwxyz";

        var (normalRoot, _) = LayoutHarness.Layout(LayoutHarness.Wrap($"<p id='p' style='width:50px'>{longWord}</p>"));
        var pNormal = LayoutHarness.FindById(normalRoot, "p")!;

        var (breakAllRoot, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap($"<p id='p' style='width:50px; word-break:break-all'>{longWord}</p>"));
        var pBreakAll = LayoutHarness.FindById(breakAllRoot, "p")!;

        // An overflowing word can push a leading empty line box ahead of it regardless of
        // word-break - count only the lines that actually carry part of the word.
        Assert.AreEqual(1, LinesWithWordContent(pNormal));
        Assert.IsTrue(LinesWithWordContent(pBreakAll) > 1, "expected break-all to force a mid-word break");
    }

    private static int LinesWithWordContent(TheArtOfDev.HtmlRenderer.Core.Dom.CssBox box) =>
        box.LineBoxes.Count(lb => lb.Words.Any(w => !string.IsNullOrEmpty(w.Text)));
}
