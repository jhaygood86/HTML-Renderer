using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Positioning;

/// <summary>
/// Port of PeachPDF.Tests' <c>FixedPositionPaginationIntegrationTests</c>, which confirms <c>position: fixed</c>
/// content repeats identically across real, multi-page <c>PdfGenerator</c> output - the "running header/footer"
/// mechanic, verified there by scanning generated PDF content streams for the fixed rect appearing on every one
/// of 3 real pages produced via <c>page-break-before: always</c>.
/// </summary>
/// <remarks>
/// This fork has no PDF-generation pipeline and no real multi-page concept in Core at all - confirmed by direct
/// source read: there is no <c>PageBreakBefore</c>/<c>PageBreakAfter</c> property anywhere (only
/// <c>PageBreakInside</c>, on <c>CssBoxProperties</c>), and the default stylesheet's own
/// <c>h1 { page-break-before: always }</c> etc. rules (<c>Core/CssDefaults.cs</c> ~102-105) are inert - there is
/// no consuming property/code path for them, so they are silently dropped by the cascade. Content instead flows
/// continuously down one (possibly very tall) canvas, and "scrolling" is the one real, working mechanism this
/// fork has for changing what's visible without re-laying-out: <c>HtmlContainerInt.ScrollOffset</c>
/// (<c>Core/HtmlContainerInt.cs</c> ~358-362).
///
/// The actual feature under test - a fixed box staying visually anchored while everything else moves - IS real
/// and working here: <c>CssBox.PaintImp</c> (<c>Core/Dom/CssBox.cs</c> ~1230-1234) only applies
/// <c>HtmlContainer.ScrollOffset</c> to a box's painted rectangle when <c>!IsFixed</c>; a fixed box's own
/// <c>Location</c> is resolved once, directly from its <c>top</c>/<c>left</c> CSS values against the page size
/// (<c>CssBoxProperties.Left</c>/<c>Top</c> setters, ~412-438, call <c>GetActualLocation</c> immediately whenever
/// <c>Position == Fixed</c>), independent of layout flow and of <c>ScrollOffset</c>. So the closest faithful
/// adaptation of "repeats identically on every page" is: paint the same laid-out tree at different
/// <c>ScrollOffset</c> values (standing in for different simulated "pages") and confirm the fixed box's painted
/// position never moves while an ordinary box's does, by exactly the scroll delta.
/// </remarks>
[DoNotParallelize]
[TestClass]
public sealed class FixedPositionPaginationIntegrationTests
{
    [TestMethod]
    public void FixedPositionedBox_PaintPosition_IsInvariantToScrollOffset_WhileNormalContentShifts()
    {
        // NOTE: uses the "background-color" longhand, not the "background" shorthand - this fork's
        // AddProperty/CssUtils dispatch has no case for the "background" shorthand key at all (confirmed by
        // source grep), so it falls through to being stored verbatim under the literal key "background",
        // which nothing ever reads - the box's real ActualBackgroundColor stays fully transparent and paints
        // no DrawRectCall at all, which is what caused this test to fail before this fix.
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='fixedBox' style='position:fixed; top:10px; left:10px; width:30px; height:30px; background-color: rgb(12,34,56);'></div>" +
            "<div id='normalBox' style='width:30px; height:30px; background-color: rgb(65,43,21);'></div>"));

        // Sanity: the fixed box really did resolve its own viewport-relative position, independent of flow.
        var fixedBox = PaintHarness.FindById(root, "fixedBox")!;
        Assert.AreEqual(10.0, fixedBox.Location.X, 0.5);
        Assert.AreEqual(10.0, fixedBox.Location.Y, 0.5);

        container.ScrollOffset = RPoint.Empty;
        var g0 = PaintHarness.PaintBox(container, root);
        var fixedRect0 = g0.Log.OfType<RecordingGraphics.DrawRectCall>().Single(r => r.Color == RColor.FromArgb(12, 34, 56));
        var normalRect0 = g0.Log.OfType<RecordingGraphics.DrawRectCall>().Single(r => r.Color == RColor.FromArgb(65, 43, 21));

        // Stand in for "the next simulated page" - a large scroll offset, the way a real page 2/3 would shift
        // the visible band of one tall continuous canvas in this fork's model.
        container.ScrollOffset = new RPoint(0, 500);
        var g1 = PaintHarness.PaintBox(container, root);
        var fixedRect1 = g1.Log.OfType<RecordingGraphics.DrawRectCall>().Single(r => r.Color == RColor.FromArgb(12, 34, 56));
        var normalRect1 = g1.Log.OfType<RecordingGraphics.DrawRectCall>().Single(r => r.Color == RColor.FromArgb(65, 43, 21));

        // position:fixed content repaints at the same screen position regardless of ScrollOffset - the real
        // mechanism behind PeachPDF's "repeats identically on every page".
        Assert.AreEqual(fixedRect0.X, fixedRect1.X, 0.5);
        Assert.AreEqual(fixedRect0.Y, fixedRect1.Y, 0.5);

        // Ordinary in-flow content visually scrolls: its painted position shifts by exactly the scroll delta.
        Assert.AreEqual(normalRect0.Y + 500, normalRect1.Y, 0.5);
    }

    [Ignore("This fork has no page-break-before/page-break-after support at all - confirmed by direct source " +
            "read: no PageBreakBefore/PageBreakAfter property exists anywhere in CssBoxProperties (only " +
            "PageBreakInside), and the default stylesheet's own page-break-before/after rules (Core/CssDefaults.cs " +
            "~102-105) are inert since nothing consumes them. 'page-break-before: always' is silently dropped, so " +
            "there is no way to materialize distinct 'pages' the way PeachPDF's real PDF pagination does - " +
            "content just flows continuously down one tall canvas and every paragraph below stacks directly " +
            "under its predecessor instead of jumping to a new page band.")]
    [TestMethod]
    public void PageBreakBefore_PushesFollowingContentToTheNextSimulatedPage()
    {
        var (root, _) = PaintHarness.Layout(PaintHarness.Wrap(
            "<p id='p1'>page 1</p>" +
            "<p id='p2' style='page-break-before: always'>page 2</p>" +
            "<p id='p3' style='page-break-before: always'>page 3</p>"));

        var p1 = PaintHarness.FindById(root, "p1")!;
        var p2 = PaintHarness.FindById(root, "p2")!;
        var p3 = PaintHarness.FindById(root, "p3")!;

        // What honoring page-break-before would look like: each paragraph starts at the top of its own page
        // band, not simply stacked directly below its predecessor.
        Assert.IsTrue(p2.Location.Y > p1.ActualBottom + 100, "expected page-break-before to push p2 onto a new page");
        Assert.IsTrue(p3.Location.Y > p2.ActualBottom + 100, "expected page-break-before to push p3 onto a new page");
    }
}
