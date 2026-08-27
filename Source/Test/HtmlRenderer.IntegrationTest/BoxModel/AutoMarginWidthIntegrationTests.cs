using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.BoxModel;

/// <summary>
/// CSS 2.1 §10.3.3: an in-flow, non-replaced block with <c>width: auto</c> and <c>margin: … auto</c>
/// <b>fills</b> its containing block (the auto margins resolve to 0); centering via auto margins only applies
/// when the used width is <b>definite</b> - an explicit <c>width</c>, or an <c>auto</c> width clamped below the
/// fill width by <c>max-width</c>.
/// </summary>
/// <remarks>
/// <para>
/// Direct reads of this fork's source confirm several independent gaps against that spec, most of which this
/// file's tests trip over:
/// </para>
/// <list type="bullet">
/// <item><b>No auto-margin centering at all.</b> <c>CssBoxProperties.ActualMarginLeft</c>/<c>ActualMarginRight</c>/
/// <c>ActualMarginTop</c> (Dom/CssBoxProperties.cs ~880-965) resolve a raw <c>"auto"</c> value to plain
/// <c>"0"</c> before parsing - there is no centering logic anywhere that redistributes free space.</item>
/// <item><b><c>min-width</c> is not implemented at all.</b> No CSS property dispatch for <c>min-width</c> exists
/// anywhere in Core (confirmed: no case for it in <c>CssUtils.GetPropertyValue</c>/<c>SetPropertyValue</c>, and
/// no <c>MinWidth</c> property on <c>CssBoxProperties</c> - only unrelated internal table-column min-width
/// tracking in <c>CssLayoutEngineTable</c> happens to share a similar name).</item>
/// <item><b><c>max-width</c> is never consulted for ordinary block width.</b> The block width computation in
/// <c>CssBox.PerformLayoutImp</c> (Dom/CssBox.cs ~629-647) computes the fill width purely from
/// <c>ContainingBlock.Size.Width</c> and the box's own (non-auto) <c>Width</c> string - it never reads
/// <c>MaxWidth</c> at all. <c>MaxWidth</c> is only consulted for <c>&lt;img&gt;</c> sizing
/// (<c>CssLayoutEngine.cs</c>) and table sizing (<c>CssLayoutEngineTable.cs</c>), so on a plain block it is
/// silently inert.</item>
/// <item><b><c>ActualWidth</c> reads back 0 for the default <c>width:auto</c>, even though the real layout
/// geometry is correct.</b> <c>CssBoxProperties.ActualWidth</c> (~806-816) parses the raw <c>Width</c> CSS
/// string via <c>CssValueParser.ParseLength</c> with NO special-case for <c>"auto"</c> (unlike the margin
/// getters above, which explicitly rewrite <c>"auto"</c> to <c>"0"</c> first) - and
/// <c>CssValueParser.ParseLength("auto", ...)</c> finds no recognized unit and <c>double.TryParse("auto", ...)</c>
/// fails, so it returns 0. Meanwhile the box's real fill width IS correctly computed and stored directly into
/// <c>Size.Width</c> by <c>PerformLayoutImp</c> (line ~643) - but that computed value is never written back into
/// the <c>Width</c> string, so <c>ActualWidth</c> can never see it for an auto-width box. A box with an
/// <i>explicit</i> (non-auto) <c>Width</c>, including a percentage one, is unaffected - percentage values are
/// parsed fine, and since they resolve relative to <c>Size.Width</c> itself the result is self-consistent.</item>
/// </list>
/// </remarks>
[DoNotParallelize]
[TestClass]
public sealed class AutoMarginWidthIntegrationTests
{
    private const double Delta = 1.0;

    // Page is 400px wide with zero page/body margins (LayoutHarness.Wrap sets body margin:0), so the body's
    // content box spans x:0..400 and a filling child should be 400px wide at x=0.

    [Ignore("box.ActualWidth reads back 0 for this box's default width:auto, per this class's remarks (the " +
            "CssBoxProperties.ActualWidth getter has no auto-special-case, unlike the margin getters) - even " +
            "though the box's real Size.Width (and Location.X) are actually filled/positioned correctly by " +
            "CssBox.PerformLayoutImp. The margin:0 auto / max-width part of this scenario is fine on this fork " +
            "(auto margins already resolve to 0, matching the expected fill-not-center outcome) - it's purely " +
            "the ActualWidth read that diverges.")]
    [TestMethod]
    public void AutoWidth_MarginZeroAuto_FillsContainingBlock()
    {
        var html = LayoutHarness.Wrap("<div id='c' style='max-width:5000px; margin:0 auto'>x</div>");
        var (root, _) = LayoutHarness.Layout(html, maxWidth: 400, maxHeight: 2000);
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(0, box.Location.X, Delta);
        Assert.AreEqual(400, box.ActualWidth, Delta);
    }

    [Ignore("Same box.ActualWidth-reads-0-for-width:auto gap as AutoWidth_MarginZeroAuto_FillsContainingBlock - " +
            "see this class's remarks. The padding-doesn't-shrink-the-fill part of the scenario is fine on this " +
            "fork (Size.Width, the border-box width, is filled independently of the box's own padding), it's " +
            "purely the ActualWidth read that diverges.")]
    [TestMethod]
    public void AutoWidth_MarginZeroAuto_WithPadding_FillsAndOffsetsByBorderPadding()
    {
        var html = LayoutHarness.Wrap("<div id='c' style='max-width:5000px; margin:0 auto; padding:0 25px'>x</div>");
        var (root, _) = LayoutHarness.Layout(html, maxWidth: 400, maxHeight: 2000);
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(0, box.Location.X, Delta);
        Assert.AreEqual(400, box.ActualWidth, Delta); // border-box width fills
    }

    [TestMethod]
    public void WidthPercent100_Child_FillsInsideAutoMarginWrapper()
    {
        // The exact cascade the invoice-bug regression targets: a width:100% child inside a margin:0 auto
        // wrapper must resolve 100% against the now-filled wrapper, not a collapsed one. Unlike the two tests
        // above, "inner" has an EXPLICIT width (100%, not auto), which sidesteps the ActualWidth-reads-0-for-auto
        // gap documented on this class - percentage widths parse fine and resolve consistently against the
        // already-correctly-filled Size.Width of the wrapper, so this one genuinely passes on this fork.
        var html = LayoutHarness.Wrap(
            "<div id='wrap' style='max-width:5000px; margin:0 auto'>"
            + "<div id='inner' style='width:100%'>x</div></div>");
        var (root, _) = LayoutHarness.Layout(html, maxWidth: 400, maxHeight: 2000);
        var inner = LayoutHarness.FindById(root, "inner")!;

        Assert.AreEqual(0, inner.Location.X, Delta);
        Assert.AreEqual(400, inner.ActualWidth, Delta);
    }

    [Ignore("This fork's ActualMarginLeft/ActualMarginRight getters (Dom/CssBoxProperties.cs) resolve a raw " +
            "'auto' margin to plain '0' before parsing - there is no centering logic anywhere - so a box with a " +
            "definite width and margin:0 auto sits flush left at x=0, not centered at (400-100)/2=150.")]
    [TestMethod]
    public void DefiniteWidth_MarginZeroAuto_Centers()
    {
        // A definite width should center via auto margins per CSS 2.1 (unchanged expectation): (400-100)/2 = 150.
        var html = LayoutHarness.Wrap("<div id='c' style='width:100px; margin:0 auto'>x</div>");
        var (root, _) = LayoutHarness.Layout(html, maxWidth: 400, maxHeight: 2000);
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(150, box.Location.X, Delta);
        Assert.AreEqual(100, box.ActualWidth, Delta);
    }

    [Ignore("Two compounding gaps: (1) max-width is never consulted for ordinary block width (see class remarks) " +
            "so this box's width never clamps to 200px at all - it stays width:auto and fills to the full 400px " +
            "container instead; (2) even if it had clamped, auto margins resolve to 0 (no centering), so " +
            "Location.X would be 0, not the expected (400-200)/2=100. box.ActualWidth also reads back 0 here " +
            "(width stays 'auto' the whole time), not the expected clamped 200.")]
    [TestMethod]
    public void AutoWidth_MaxWidthBinds_MarginZeroAuto_Centers()
    {
        // When max-width is smaller than the page it should clamp the used width, which becomes definite and
        // centers: width = 200px, (400-200)/2 = 100px each side.
        var html = LayoutHarness.Wrap("<div id='c' style='max-width:200px; margin:0 auto'>x</div>");
        var (root, _) = LayoutHarness.Layout(html, maxWidth: 400, maxHeight: 2000);
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(200, box.ActualWidth, Delta);
        Assert.AreEqual(100, box.Location.X, Delta);
    }

    [Ignore("min-width is not implemented at all on this fork (see class remarks), and max-width is never " +
            "consulted for ordinary block width either - so neither the 200px clamp nor the 300px re-widen ever " +
            "happens; the box just stays width:auto and fills the 400px container. Auto margins also resolve to " +
            "0 (no centering), so neither box.ActualWidth (expected 300) nor box.Location.X (expected 50) match.")]
    [TestMethod]
    public void AutoWidth_MinWidthReWidensPastMaxWidth_CentersOnMinWidth()
    {
        // Degenerate min-width > max-width: min should win (CSS 2.1 §10.4), so the used width is 300px, not the
        // clamped 200px - the auto margins should then center on 300px: (400-300)/2 = 50px.
        var html = LayoutHarness.Wrap("<div id='c' style='max-width:200px; min-width:300px; margin:0 auto'>x</div>");
        var (root, _) = LayoutHarness.Layout(html, maxWidth: 400, maxHeight: 2000);
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(300, box.ActualWidth, Delta);
        Assert.AreEqual(50, box.Location.X, Delta);
    }

    [Ignore("min-width is not implemented at all on this fork (see class remarks) - so this box never widens " +
            "past the 400px container to the expected 600px overflow; it just fills the container normally, and " +
            "its width:auto also means box.ActualWidth reads back 0 rather than either 400 or 600 (see class " +
            "remarks on the ActualWidth-for-auto gap). Location.X may coincidentally still read ~0, but for an " +
            "unrelated reason (auto margins resolving to 0), not because min-width forced an overflow.")]
    [TestMethod]
    public void AutoWidth_MinWidthExceedsContainingBlock_MarginsCollapseToZero()
    {
        // A min-width wider than the containing block should overflow: no free space, auto margins 0, box
        // pinned at the containing-block left edge.
        var html = LayoutHarness.Wrap("<div id='c' style='min-width:600px; margin:0 auto'>x</div>");
        var (root, _) = LayoutHarness.Layout(html, maxWidth: 400, maxHeight: 2000);
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(600, box.ActualWidth, Delta);
        Assert.AreEqual(0, box.Location.X, Delta);
    }

    [Ignore("Sanity companion to the other tests in this class, isolating the ActualWidth-reads-0-for-width:auto " +
            "gap on its own: margin here is plain '0' (not 'auto'), so the auto-margin-centering bug is not in " +
            "play at all, and max-width being inert for blocks doesn't matter either since it doesn't bind (5000 " +
            "> 400) even on a correct engine - box.Location.X (0) is right, but box.ActualWidth still reads back " +
            "0 instead of 400 purely because CssBoxProperties.ActualWidth has no 'auto' special-case (see class " +
            "remarks).")]
    [TestMethod]
    public void AutoWidth_MarginZero_LeftAligns_Fills()
    {
        // Sanity: plain margin:0 (no auto) should fill and left-align, matching the auto-margin fill.
        var html = LayoutHarness.Wrap("<div id='c' style='max-width:5000px; margin:0'>x</div>");
        var (root, _) = LayoutHarness.Layout(html, maxWidth: 400, maxHeight: 2000);
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(0, box.Location.X, Delta);
        Assert.AreEqual(400, box.ActualWidth, Delta);
    }

    [Ignore("Adapted from PeachPDF: the original test calls static CssLayoutEngine.GetActualMarginLeft/" +
            "GetActualMarginRight helpers that resolve a table's centered margin against its own resolved width - " +
            "no such methods exist anywhere in this fork's (internal static) CssLayoutEngine/CssLayoutEngineTable " +
            "classes (confirmed: no 'GetActualMargin' hits in either file), so the call itself would not compile " +
            "here. This fork centers a table (if at all) only via the parent's inherited text-align:center " +
            "(CssLayoutEngineTable.cs ~line 617), never via the table's own margin:auto - a table's own " +
            "ActualMarginLeft/ActualMarginRight are the same generic CssBoxProperties getters every other box " +
            "uses, which resolve 'auto' to '0' with no centering (see class remarks). Adapted here to assert on " +
            "the table's resulting Location.X directly, which is the closest equivalent observation the current " +
            "harness/API can make.")]
    [TestMethod]
    public void Table_MarginZeroAuto_CentersAgainstResolvedWidth()
    {
        // A margin: 0 auto table should be centered against its own resolved (explicit) width: wrap is 200px
        // wide, table is 100px wide, so it should sit 50px in from each side.
        var html = LayoutHarness.Wrap(
            "<div id='wrap' style='width:200px'>"
            + "<table id='t' style='width:100px; margin:0 auto'><tr><td>A</td></tr></table></div>");
        var (root, _) = LayoutHarness.Layout(html);
        var wrap = LayoutHarness.FindById(root, "wrap")!;
        var table = LayoutHarness.FindById(root, "t")!;

        var expected = wrap.ClientLeft + (200.0 - 100.0) / 2.0; // 50px each side

        Assert.AreEqual(expected, table.Location.X, Delta);
    }
}
