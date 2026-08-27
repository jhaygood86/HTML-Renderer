using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Text;

/// <summary>
/// Regression coverage for <c>font-variant: small-caps</c>. Ported from PeachPDF's
/// <c>SmallCapsIntegrationTests</c>, which covers a real small-caps synthesis pipeline (originally-lowercase
/// runs upper-cased and re-measured/painted at a reduced size via <c>DerivedStyle.ActualSmallCapsFont</c>,
/// exposed on <c>CssRectWord</c> via <c>FontSizeScale</c>/<c>SuppressWrapBefore</c>, and a dedicated
/// <c>RecordingGraphics</c> paint harness asserting <c>DrawString</c> call order/fonts).
/// </summary>
/// <remarks>
/// HTML-Renderer has none of that: <c>font-variant</c> IS parsed/stored as a plain string property
/// (<see cref="CssBoxProperties.FontVariant"/>, default "normal", inherited - recognized via both the
/// standalone <c>font-variant</c> property and the <c>font</c> shorthand regex in
/// <c>CssParser.ParseFontProperty</c>), but a full-tree grep across <c>CssLayoutEngine.cs</c> and the
/// font/paint code (<c>ActualFont</c>, <c>RFontStyle</c> construction) found zero non-storage read sites -
/// it is a complete no-op beyond storage. There is also no <c>font-variant-caps</c>/<c>all-small-caps</c>
/// support at all (not a recognized property name anywhere in this fork).
/// <para>
/// Confirmed by direct execution against the built assembly (not just source reading): laying out
/// <c>&lt;b style="font-variant:small-caps"&gt;Hello&lt;/b&gt;</c> leaves the box with a single, unsplit
/// "Hello" word - the same as with no <c>font-variant</c> at all.
/// </para>
/// <para>
/// Because this fork has no equivalent of <c>FontSizeScale</c>/<c>SmallCapsFontScale</c>/
/// <c>ActualSmallCapsFont</c>/<c>SuppressWrapBefore</c> (confirmed absent by grep - not merely unused, the
/// members do not exist), PeachPDF's scale/measured-width/wrap-suppression/space-flag-on-fragment/paint-call
/// tests have no faithful, compilable equivalent here and are intentionally not ported (porting a test file
/// cannot invent new production API surface). What IS ported below is: (a) a parse/storage check, since that
/// part of the property genuinely still works, and (b) the word-splitting expectation itself - the one
/// observable signal shared by every PeachPDF case - both for real small-caps and for the (unsupported)
/// all-small-caps spelling.
/// </para>
/// </remarks>
// This fork's CssParser keeps a process-wide, non-thread-safe regex cache
// (RegexParserUtils.GetRegex's static Dictionary) that SetHtml/DefaultCssData populate lazily on first use per
// AppDomain; running HtmlContainerInt.SetHtml from more than one thread at once (as MSTestSettings.cs's
// assembly-wide [Parallelize(Scope = ExecutionScope.MethodLevel)] does by default) can corrupt it and throw
// "A concurrent update was performed on this collection". [DoNotParallelize] avoids tripping that pre-existing
// library race rather than masking it.
[DoNotParallelize]
[TestClass]
public sealed class SmallCapsIntegrationTests
{
    /// <summary>Finds the box that actually owns the word(s): text nodes get their own anonymous child
    /// <see cref="CssBox"/> (<c>DomParser.CorrectTextBoxes</c>), so an element like
    /// <c>&lt;b id="w"&gt;Hello&lt;/b&gt;</c>'s own box has an empty <see cref="CssBox.Words"/> - the words
    /// live on its single anonymous text child instead.</summary>
    private static CssBox FindWordsBox(CssBox root, string id)
    {
        var element = LayoutHarness.FindById(root, id)!;
        if (element.Words.Count > 0) return element;

        var wordsChild = element.Boxes.FirstOrDefault(b => b.Words.Count > 0);
        Assert.IsNotNull(wordsChild, $"no descendant of #{id} owns any words");
        return wordsChild!;
    }

    [TestMethod]
    public void FontVariant_SmallCaps_StandaloneProperty_ParsesAndStores()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<b id='w' style='font-variant:small-caps'>Hello</b>"));
        var w = LayoutHarness.FindById(root, "w")!;

        Assert.AreEqual("small-caps", w.FontVariant);
    }

    [TestMethod]
    public void FontVariant_SmallCaps_ViaFontShorthand_ParsesAndStores()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<b id='w' style='font:italic small-caps bold 12px Arial'>Hello</b>"));
        var w = LayoutHarness.FindById(root, "w")!;

        Assert.AreEqual("small-caps", w.FontVariant);
    }

    [Ignore("HTML-Renderer's font-variant is storage-only - CssBox.FontVariant is set but never read anywhere " +
            "in layout or paint (confirmed by grep and by direct execution: the word stays a single unsplit " +
            "'Hello', not split into 'H' + 'ELLO' the way PeachPDF's synthesis pipeline produces). Real word " +
            "splitting/scaling is out of scope for this fork; see class remarks.")]
    [TestMethod]
    public void SmallCaps_SplitsWordIntoCaseRuns()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<b id='w' style='font-variant:small-caps'>Hello</b>"));
        var box = FindWordsBox(root, "w");

        // "Hello" -> "H" (already upper) + "ELLO" (synthesized small-caps run), per PeachPDF's real behavior.
        Assert.AreEqual(2, box.Words.Count);
        Assert.AreEqual("H", box.Words[0].Text);
        Assert.AreEqual("ELLO", box.Words[1].Text);
    }

    [TestMethod]
    public void NoSmallCaps_WordIsNotSplit_Regression()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<b id='w'>Hello</b>"));
        var box = FindWordsBox(root, "w");

        Assert.AreEqual(1, box.Words.Count);
        Assert.AreEqual("Hello", box.Words[0].Text);
    }

    [TestMethod]
    public void SmallCaps_WordWithNoLowercaseLetters_IsNotSplit()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<b id='w' style='font-variant:small-caps'>ABC</b>"));
        var box = FindWordsBox(root, "w");

        Assert.AreEqual(1, box.Words.Count);
        Assert.AreEqual("ABC", box.Words[0].Text);
    }

    // ─── font-variant-caps / all-small-caps: not a recognized property name anywhere in this fork (only the
    // standalone "font-variant" property and its "normal|small-caps" values are wired up), so these always
    // behave identically to plain unset font-variant - confirmed via grep, no case in CssUtils's property
    // switch (get or set) mentions "font-variant-caps" at all. ────────────────────────────────────────────

    [TestMethod]
    public void AllSmallCaps_WordWithNoLowercaseLetters_WordStaysIntact()
    {
        // PeachPDF: font-variant-caps:all-small-caps shrinks an already-uppercase word (c2sc approximation).
        // Here font-variant-caps isn't a recognized property at all, so this reduces to a plain unsplit-word
        // regression check - it is not exercising any shrinking behavior.
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<b id='w' style='font-variant-caps:all-small-caps'>ABC</b>"));
        var box = FindWordsBox(root, "w");

        Assert.AreEqual(1, box.Words.Count);
        Assert.AreEqual("ABC", box.Words[0].Text);
    }

    [TestMethod]
    public void AllSmallCaps_WordWithNoLetters_IsNotSplit()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<b id='w' style='font-variant-caps:all-small-caps'>123</b>"));
        var box = FindWordsBox(root, "w");

        Assert.AreEqual(1, box.Words.Count);
        Assert.AreEqual("123", box.Words[0].Text);
    }

    [Ignore("font-variant-caps isn't a recognized property in this fork (grep-confirmed absent from CssUtils's " +
            "property switch), so there is no c2sc/small-caps approximation to split a mixed-case word into " +
            "upper/lower runs - the word stays a single unsplit 'AbC', not the three runs " +
            "('A','B','C') PeachPDF's synthesis produces.")]
    [TestMethod]
    public void AllSmallCaps_MixedCaseWord_WouldSplitIntoThreeRuns()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<b id='w' style='font-variant-caps:all-small-caps'>AbC</b>"));
        var box = FindWordsBox(root, "w");

        Assert.AreEqual(3, box.Words.Count);
        Assert.AreEqual("A", box.Words[0].Text);
        Assert.AreEqual("B", box.Words[1].Text);
        Assert.AreEqual("C", box.Words[2].Text);
    }

    [Ignore("font-variant-caps isn't a recognized property in this fork, so there is no run-splitting at all - " +
            "the word stays a single unsplit 'a1b', not the three runs ('A','1','B') PeachPDF's synthesis " +
            "produces around the non-lowercase digit run.")]
    [TestMethod]
    public void AllSmallCaps_DigitRunBetweenLowercaseRuns_WouldSplitIntoThreeRuns()
    {
        var (root, _) = LayoutHarness.Layout(
            LayoutHarness.Wrap("<b id='w' style='font-variant-caps:all-small-caps'>a1b</b>"));
        var box = FindWordsBox(root, "w");

        Assert.AreEqual(3, box.Words.Count);
        Assert.AreEqual("A", box.Words[0].Text);
        Assert.AreEqual("1", box.Words[1].Text);
        Assert.AreEqual("B", box.Words[2].Text);
    }
}
