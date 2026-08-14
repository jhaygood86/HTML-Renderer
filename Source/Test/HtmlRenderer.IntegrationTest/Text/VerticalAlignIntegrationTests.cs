using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Text;

/// <summary>
/// Verifies whether <c>vertical-align</c> actually repositions inline-level content relative to its line box.
/// Ported from PeachPDF's <c>VerticalAlignIntegrationTests</c>, whose header describes a fixed bug: in
/// PeachPDF, <c>top</c>/<c>bottom</c>/<c>middle</c>/<c>text-top</c>/<c>text-bottom</c> used to hit an empty
/// case in <c>CssLayoutEngine.ApplyVerticalAlignment</c> and were silent no-ops, while <c>baseline</c>/
/// <c>sub</c>/<c>super</c> (and, in PeachPDF, table cells via the separate <c>ApplyCellVerticalAlignment</c>)
/// genuinely worked.
/// </summary>
/// <remarks>
/// <para>
/// HTML-Renderer's <c>ApplyVerticalAlignment</c> (<c>Core/Dom/CssLayoutEngine.cs</c>) has the same empty
/// <c>case</c> bodies for <c>top</c>/<c>bottom</c>/<c>middle</c>/<c>text-top</c>/<c>text-bottom</c> that
/// PeachPDF used to have, and real logic for <c>sub</c>/<c>super</c>/<c>baseline</c> (via
/// <c>CssLineBox.SetBaseLine</c>). <b>But confirmed by direct execution against the built assembly, that
/// <c>sub</c>/<c>super</c> logic never actually moves ordinary inline content either</b> - for the completely
/// standard <c>&lt;span style="vertical-align:sub"&gt;text&lt;/span&gt;</c> shape this file's helper uses
/// (and PeachPDF's did too), two things combine to make it a no-op:
/// </para>
/// <list type="number">
/// <item>Text is always split into its own anonymous child <c>CssBox</c>
/// (<c>DomParser.CorrectTextBoxes</c>) - the <c>&lt;span&gt;</c> itself owns no <c>CssBox.Words</c>
/// directly, its anonymous text child does.</item>
/// <item><c>vertical-align</c> is not copied by the normal (non-"everything") overload of
/// <c>CssBoxProperties.InheritStyle</c> (<c>Core/Dom/CssBoxProperties.cs</c>, ~line 1490-1565) - so that
/// anonymous text child never inherits the span's <c>vertical-align</c> and keeps the default "baseline"
/// case.</item>
/// </list>
/// <para>
/// <c>CssLineBox.SetBaseLine(g, box, baseline)</c> only ever moves the words returned by
/// <c>WordsOf(box)</c> (an exact <c>word.OwnerBox == box</c> match). So the switch's <c>sub</c>/<c>super</c>
/// branches do run for the <c>&lt;span&gt;</c> itself (which has the real <c>vertical-align</c> value) but
/// touch zero words (<c>WordsOf(span)</c> is empty), while the branch that runs for the anonymous text child
/// (which owns the real word) always takes the <c>default</c>/baseline case, because that child's own
/// <c>vertical-align</c> was never inherited. Net result, confirmed empirically: laying out eleven variants
/// (<c>top</c>, <c>bottom</c>, <c>middle</c>, <c>text-top</c>, <c>text-bottom</c>, <c>sub</c>, <c>super</c>,
/// <c>baseline</c>, and numeric/percentage lengths, none of which have any <c>case</c> in the switch at all)
/// produced the exact same word <c>Top</c> in every case - inline <c>vertical-align</c> is a total no-op in
/// this fork for the ordinary "element wraps a text node" markup shape.
/// </para>
/// <para>
/// Table cells are unaffected by any of this: <c>ApplyCellVerticalAlignment</c> is a separate code path that
/// calls <c>b.OffsetTop(dist)</c> directly on every child box of the cell (bypassing <c>WordsOf</c> and
/// inheritance entirely), and <c>top</c>/<c>middle</c>/<c>bottom</c> there are confirmed genuinely working by
/// direct execution (distinct word tops for top/middle/bottom, with middle landing exactly at the midpoint) -
/// so the two table-cell cases below are ported as real, non-ignored tests.
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
public sealed class VerticalAlignIntegrationTests
{
    private const double Delta = 0.5;

    private const string InlineNoOpReason =
        "HTML-Renderer's inline vertical-align is a total no-op for the standard <span>text</span> shape " +
        "used here - confirmed by direct execution: the span's own Words collection is always empty (text " +
        "lives on an anonymous child box instead), and that anonymous child never inherits vertical-align " +
        "(CssBoxProperties.InheritStyle's normal overload does not copy it), so it always takes the " +
        "default/baseline case in CssLayoutEngine.ApplyVerticalAlignment regardless of what the span's own " +
        "vertical-align was set to. See class remarks for the full mechanism.";

    private static double GetAlignedTop(string verticalAlign, string? lineHeight = null)
    {
        // "v" is deliberately smaller than its parent's own font (10px vs 16px) - text-top/text-bottom align
        // with the *parent's* font box, and if the aligned box were taller than that reference box, "top"
        // and "bottom" alignment could legitimately cross over.
        var lineHeightDecl = lineHeight is null ? "" : $"; line-height:{lineHeight}";
        var html = LayoutHarness.Wrap(
            "<p id='p' style='font-size:16px'><span style='font-size:60px'>TALL</span> " +
            $"<span id='v' style='vertical-align:{verticalAlign}; font-size:10px{lineHeightDecl}'>small</span></p>");

        var (root, _) = LayoutHarness.Layout(html);
        var v = LayoutHarness.FindById(root, "v")!;
        return LayoutHarness.Descendants(v).SelectMany(b => b.Words).First().Top;
    }

    [Ignore(InlineNoOpReason)]
    [TestMethod]
    public void Top_PositionsHigherThanBottom()
    {
        var topY = GetAlignedTop("top");
        var bottomY = GetAlignedTop("bottom");

        Assert.IsTrue(topY < bottomY, $"expected top-aligned span ({topY}) to sit above bottom-aligned span ({bottomY})");
    }

    [Ignore(InlineNoOpReason)]
    [TestMethod]
    public void Middle_PositionsBetweenTopAndBottom()
    {
        var topY = GetAlignedTop("top");
        var bottomY = GetAlignedTop("bottom");
        var middleY = GetAlignedTop("middle");

        Assert.IsTrue(middleY > topY && middleY < bottomY);
    }

    [Ignore(InlineNoOpReason)]
    [TestMethod]
    public void TextTop_PositionsAboveTextBottom()
    {
        var textTopY = GetAlignedTop("text-top");
        var textBottomY = GetAlignedTop("text-bottom");

        Assert.IsTrue(textTopY < textBottomY, $"top={textTopY} bottom={textBottomY}");
    }

    [Ignore(InlineNoOpReason)]
    [TestMethod]
    public void Sub_PositionsBelowSuper()
    {
        var subY = GetAlignedTop("sub");
        var superY = GetAlignedTop("super");

        Assert.IsTrue(subY > superY, $"sub={subY} super={superY}");
    }

    [Ignore(InlineNoOpReason)]
    [TestMethod]
    public void Bottom_DiffersFromDefaultBaselineAlignment()
    {
        var bottomY = GetAlignedTop("bottom");
        var baselineY = GetAlignedTop("baseline");

        Assert.AreNotEqual(baselineY, bottomY);
    }

    [Ignore(InlineNoOpReason)]
    [TestMethod]
    public void Middle_DiffersFromDefaultBaselineAlignment()
    {
        var middleY = GetAlignedTop("middle");
        var baselineY = GetAlignedTop("baseline");

        Assert.AreNotEqual(baselineY, middleY);
    }

    [Ignore(InlineNoOpReason)]
    [TestMethod]
    public void TextTop_ReferencesParentFontAscent_NotJustLineTop()
    {
        // text-top aligns with the top of the *parent's* font (CSS1 §5.6.11), not the line's raw top extent
        // the way plain "top" does - changing only the parent's font-size (the target span stays fixed at
        // 10px in both builds) must still move the result, proving the parent's font metrics are actually
        // consulted rather than this collapsing to plain "top".
        var htmlSmallParent = LayoutHarness.Wrap(
            "<p id='p' style='font-size:10px'><span style='font-size:60px'>TALL</span> " +
            "<span id='v' style='vertical-align:text-top; font-size:10px'>small</span></p>");
        var htmlLargeParent = LayoutHarness.Wrap(
            "<p id='p' style='font-size:40px'><span style='font-size:60px'>TALL</span> " +
            "<span id='v' style='vertical-align:text-top; font-size:10px'>small</span></p>");

        var (rootSmall, _) = LayoutHarness.Layout(htmlSmallParent);
        var (rootLarge, _) = LayoutHarness.Layout(htmlLargeParent);

        var vSmall = LayoutHarness.FindById(rootSmall, "v")!;
        var vLarge = LayoutHarness.FindById(rootLarge, "v")!;
        var ySmall = LayoutHarness.Descendants(vSmall).SelectMany(b => b.Words).First().Top;
        var yLarge = LayoutHarness.Descendants(vLarge).SelectMany(b => b.Words).First().Top;

        Assert.AreNotEqual(ySmall, yLarge);
    }

    [TestMethod]
    public void Bottom_OnATableCell_PushesShortContentLowerThanTopAligned()
    {
        // CssLayoutEngine.ApplyCellVerticalAlignment's table-specific alignment algorithm (distinct from the
        // inline ApplyVerticalAlignment exercised by the other tests in this file) - a short cell in a taller
        // row must be pushed all the way to the row's bottom under vertical-align:bottom, unlike
        // vertical-align:top where it stays put.
        var htmlTop = LayoutHarness.Wrap(
            "<table><tr>"
            + "<td style='height:100px'>Tall</td>"
            + "<td id='v' style='vertical-align:top'>Short</td>"
            + "</tr></table>");
        var htmlBottom = LayoutHarness.Wrap(
            "<table><tr>"
            + "<td style='height:100px'>Tall</td>"
            + "<td id='v' style='vertical-align:bottom'>Short</td>"
            + "</tr></table>");

        var (rootTop, _) = LayoutHarness.Layout(htmlTop);
        var (rootBottom, _) = LayoutHarness.Layout(htmlBottom);

        var topY = LayoutHarness.Descendants(LayoutHarness.FindById(rootTop, "v")!).SelectMany(b => b.Words).First().Top;
        var bottomY = LayoutHarness.Descendants(LayoutHarness.FindById(rootBottom, "v")!).SelectMany(b => b.Words).First().Top;

        Assert.IsTrue(bottomY > topY,
            $"vertical-align:bottom ({bottomY}) should push the cell's content lower than vertical-align:top ({topY})");
    }

    [TestMethod]
    public void Middle_OnATableCellWithExplicitHeight_CentersShortContent()
    {
        var htmlTop = LayoutHarness.Wrap(
            "<table><tr>"
            + "<td style='height:100px'>Tall</td>"
            + "<td id='v' style='height:100px; vertical-align:top'>Short</td>"
            + "</tr></table>");
        var htmlMiddle = LayoutHarness.Wrap(
            "<table><tr>"
            + "<td style='height:100px'>Tall</td>"
            + "<td id='v' style='height:100px; vertical-align:middle'>Short</td>"
            + "</tr></table>");
        var htmlBottom = LayoutHarness.Wrap(
            "<table><tr>"
            + "<td style='height:100px'>Tall</td>"
            + "<td id='v' style='height:100px; vertical-align:bottom'>Short</td>"
            + "</tr></table>");

        var (rootTop, _) = LayoutHarness.Layout(htmlTop);
        var (rootMiddle, _) = LayoutHarness.Layout(htmlMiddle);
        var (rootBottom, _) = LayoutHarness.Layout(htmlBottom);

        var topY = LayoutHarness.Descendants(LayoutHarness.FindById(rootTop, "v")!).SelectMany(b => b.Words).First().Top;
        var middleY = LayoutHarness.Descendants(LayoutHarness.FindById(rootMiddle, "v")!).SelectMany(b => b.Words).First().Top;
        var bottomY = LayoutHarness.Descendants(LayoutHarness.FindById(rootBottom, "v")!).SelectMany(b => b.Words).First().Top;

        Assert.IsTrue(middleY > topY && middleY < bottomY,
            $"vertical-align:middle ({middleY}) should land strictly between vertical-align:top ({topY}) and vertical-align:bottom ({bottomY}) even with an explicit cell height");

        // ApplyCellVerticalAlignment splits the leftover room evenly for `middle` (half of what `bottom`
        // moves it by), so middle must sit at the exact midpoint.
        Assert.AreEqual((topY + bottomY) / 2, middleY, Delta);
    }

    [Ignore(InlineNoOpReason + " Numeric/percentage lengths have no case at all in the switch, so they fall " +
            "to the same no-op default path.")]
    [TestMethod]
    public void Length_PositiveValue_RaisesTheBoxAboveBaseline()
    {
        // CSS 2.1 §10.8.1: a positive length raises the box by that distance from its own baseline.
        var baselineY = GetAlignedTop("baseline");
        var raisedY = GetAlignedTop("5px");

        Assert.IsTrue(raisedY < baselineY, $"raised={raisedY} baseline={baselineY}");
    }

    [Ignore(InlineNoOpReason + " Numeric/percentage lengths have no case at all in the switch, so they fall " +
            "to the same no-op default path.")]
    [TestMethod]
    public void Length_NegativeValue_LowersTheBoxBelowBaseline()
    {
        var baselineY = GetAlignedTop("baseline");
        var loweredY = GetAlignedTop("-5px");

        Assert.IsTrue(loweredY > baselineY, $"lowered={loweredY} baseline={baselineY}");
    }

    [Ignore(InlineNoOpReason + " Numeric/percentage lengths have no case at all in the switch, so they fall " +
            "to the same no-op default path.")]
    [TestMethod]
    public void Percentage_PositiveValue_RaisesTheBoxAboveBaseline()
    {
        var baselineY = GetAlignedTop("baseline");
        var raisedY = GetAlignedTop("50%");

        Assert.IsTrue(raisedY < baselineY, $"raised={raisedY} baseline={baselineY}");
    }

    [Ignore(InlineNoOpReason + " Numeric/percentage lengths have no case at all in the switch, so they fall " +
            "to the same no-op default path.")]
    [TestMethod]
    public void Percentage_ResolvesAgainstTheBoxsOwnLineHeight()
    {
        // A percentage is a fraction of the box's own line-height (CSS 2.1 §10.8.1) - doubling the
        // line-height (everything else unchanged) must double the raise relative to that line-height's own
        // baseline, proving the percentage is actually resolved against it rather than some other fixed
        // reference (e.g. font-size).
        var baseline20 = GetAlignedTop("baseline", lineHeight: "20px");
        var percent20 = GetAlignedTop("50%", lineHeight: "20px");
        var baseline40 = GetAlignedTop("baseline", lineHeight: "40px");
        var percent40 = GetAlignedTop("50%", lineHeight: "40px");

        var raise20 = baseline20 - percent20;
        var raise40 = baseline40 - percent40;

        Assert.AreEqual(raise20 * 2, raise40, Delta);
    }
}
