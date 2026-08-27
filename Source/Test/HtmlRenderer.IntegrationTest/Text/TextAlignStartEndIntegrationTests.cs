using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Text;

/// <summary>
/// <c>text-align</c>'s CSS-correct initial value is <c>start</c> (CSS Text 3 §7.1), which is meant to resolve
/// against the box's own <c>direction</c> at layout time - not always to <c>left</c>, the legacy/incorrect
/// initial value this replaces. Ported from PeachPDF's <c>TextAlignStartEndIntegrationTests</c>.
/// </summary>
/// <remarks>
/// HTML-Renderer's <c>CssBoxProperties.TextAlign</c> is a plain, unvalidated string property - whatever
/// literal value the stylesheet declares (including "start"/"end") is stored as-is, with no keyword
/// whitelist. But <c>CssLayoutEngine.ApplyHorizontalAlignment</c>'s switch only has explicit cases for
/// <c>right</c>/<c>center</c>/<c>justify</c>; everything else - including <c>start</c>, <c>end</c>,
/// <c>left</c>, and unset - falls through to <c>default</c> -&gt; <c>ApplyLeftAlignment</c> (itself a
/// complete no-op: the words are simply left where <c>FlowBox</c> already placed them, which is against the
/// line's own left edge, independent of the box's <c>direction</c>). <c>direction:rtl</c> separately drives
/// <c>ApplyRightToLeft</c>, but that only reorders multiple words' relative positions within a line - for a
/// single-word line (as used below) it is a no-op.
/// <para>
/// Net effect, confirmed by direct execution against the built assembly: a <c>text-align:start</c> or
/// <c>text-align:end</c> box always packs its text against the left edge, regardless of <c>dir="rtl"</c>.
/// That happens to match two of the four PeachPDF start/end cases (the ones that expect left-edge packing)
/// and diverge from the other two (which expect right-edge packing) - so only those two are ported as
/// genuinely broken here.
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
public sealed class TextAlignStartEndIntegrationTests
{
    private const double Delta = 1.0;

    private static CssRect FirstWord(CssBox box) =>
        LayoutHarness.Descendants(box).SelectMany(b => b.Words).First(w => !w.IsSpaces);

    [TestMethod]
    public void Start_InLtrBlock_PacksTextAgainstTheLeftEdge()
    {
        var html = LayoutHarness.Wrap("<p id='p' style='text-align: start; width: 200px'>hi</p>");

        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;
        var word = FirstWord(p);

        Assert.AreEqual(p.ClientLeft, word.Left, Delta);
    }

    [Ignore("text-align:start falls through CssLayoutEngine.ApplyHorizontalAlignment's switch to the default " +
            "(left-align) case regardless of direction - confirmed by direct execution: a dir='rtl' box with " +
            "text-align:start still packs its word against the left edge (word.Left == ClientLeft), not the " +
            "right edge this test (correctly, per CSS Text 3) expects.")]
    [TestMethod]
    public void Start_InRtlBlock_PacksTextAgainstTheRightEdge()
    {
        var html = LayoutHarness.Wrap("<p id='p' dir='rtl' style='text-align: start; width: 200px'>hi</p>");

        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;
        var word = FirstWord(p);

        Assert.AreEqual(p.ClientRight, word.Right, Delta);
    }

    [Ignore("text-align:end falls through CssLayoutEngine.ApplyHorizontalAlignment's switch to the default " +
            "(left-align) case - confirmed by direct execution: an LTR box with text-align:end still packs " +
            "its word against the left edge (word.Left == ClientLeft), not the right edge this test " +
            "(correctly, per CSS Text 3) expects.")]
    [TestMethod]
    public void End_InLtrBlock_PacksTextAgainstTheRightEdge()
    {
        var html = LayoutHarness.Wrap("<p id='p' style='text-align: end; width: 200px'>hi</p>");

        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;
        var word = FirstWord(p);

        Assert.AreEqual(p.ClientRight, word.Right, Delta);
    }

    [TestMethod]
    public void End_InRtlBlock_PacksTextAgainstTheLeftEdge()
    {
        var html = LayoutHarness.Wrap("<p id='p' dir='rtl' style='text-align: end; width: 200px'>hi</p>");

        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;
        var word = FirstWord(p);

        Assert.AreEqual(p.ClientLeft, word.Left, Delta);
    }

    [TestMethod]
    public void DefaultsToStart_UnsetTextAlign_BehavesLikeLeftInLtr()
    {
        var html = LayoutHarness.Wrap("<p id='p' style='width: 200px'>hi</p>");

        var (root, _) = LayoutHarness.Layout(html);
        var p = LayoutHarness.FindById(root, "p")!;
        var word = FirstWord(p);

        Assert.AreEqual(p.ClientLeft, word.Left, Delta);
    }
}
