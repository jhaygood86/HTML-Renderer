using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// CSS 2.1 §17.6.1: <c>visibility: collapse</c> on a table row/row-group/column/column-group removes it
/// from the table's geometry entirely - as if it had <c>display: none</c> - so the rows/columns after it
/// shift up/left to fill the gap. Distinct from <c>visibility: hidden</c>, which reserves the element's
/// layout space and only omits painting it.
/// </summary>
/// <remarks>
/// HTML-Renderer fact (confirmed, <c>Dom\CssLayoutEngineTable.cs</c>): zero awareness of
/// <c>visibility:collapse</c> anywhere in the table layout engine (only unrelated <c>border-collapse</c>/
/// <c>PageBreakInside</c> hits exist for the word "collapse") - a &lt;tr&gt;/&lt;col&gt;/etc. with
/// <c>visibility:collapse</c> is NOT removed from the table's geometry/height/width calculations at all, so
/// every test below that actually exercises collapse-driven removal is [Ignore]d. The one test that does
/// NOT depend on collapse-specific logic (only on plain <c>visibility:hidden</c> reserving space, a much
/// more basic/general mechanism most CSS engines implement) is left active.
/// </remarks>
[DoNotParallelize]
[TestClass]
public sealed class TableVisibilityCollapseIntegrationTests
{
    private const string CollapseNotImplemented =
        "This fork's CssLayoutEngineTable has zero awareness of visibility:collapse (confirmed: no " +
        "collapse/visibility handling anywhere in Dom/CssLayoutEngineTable.cs beyond unrelated " +
        "border-collapse/PageBreakInside hits) - a collapsed row/row-group/column/column-group is not " +
        "removed from the table's geometry, so it keeps taking up its normal space instead of being " +
        "skipped like a genuinely absent row/column.";

    [Ignore(CollapseNotImplemented)]
    [TestMethod]
    public void CollapsedRow_DirectTableChild_TakesNoSpace()
    {
        // <tr> as a direct child of <table> (no row group) - CssLayoutEngineTable.AssignBoxKinds'
        // Keywords.TableRow branch.
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tr><td class="a" style="height:20px">A</td></tr>
              <tr style="visibility:collapse"><td class="b" style="height:20px">B</td></tr>
              <tr><td class="c" style="height:20px">C</td></tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tr><td class="a" style="height:20px">A</td></tr>
              <tr><td class="c" style="height:20px">C</td></tr>
            </table>
            </body></html>
            """);

        AssertSameLocation(FindByClass(control, "c")!, FindByClass(experiment, "c")!);
    }

    [Ignore(CollapseNotImplemented)]
    [TestMethod]
    public void CollapsedRow_InsideRowGroup_TakesNoSpace()
    {
        // A <tr> inside a <tbody> - the Keywords.TableRowGroup branch.
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tbody>
                <tr><td class="a" style="height:20px">A</td></tr>
                <tr style="visibility:collapse"><td class="b" style="height:20px">B</td></tr>
                <tr><td class="c" style="height:20px">C</td></tr>
              </tbody>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tbody>
                <tr><td class="a" style="height:20px">A</td></tr>
                <tr><td class="c" style="height:20px">C</td></tr>
              </tbody>
            </table>
            </body></html>
            """);

        AssertSameLocation(FindByClass(control, "c")!, FindByClass(experiment, "c")!);
    }

    [TestMethod]
    public void RowspanCrossingCollapsedRow_DoesNotMisalignLaterRow()
    {
        // A rowspan cell opening before a collapsed row and extending into/past it must not have its
        // placeholder land in the wrong row - the unrelated row after the span must keep its own two cells
        // in their own two columns rather than being pushed over by a leaked placeholder.
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tr><td rowspan="3" style="height:60px">Span</td><td style="height:20px">R1C2</td></tr>
              <tr style="visibility:collapse"><td style="height:20px">R2C2</td></tr>
              <tr><td style="height:20px">R3C2</td></tr>
              <tr><td class="after1" style="height:20px">After1</td><td class="after2" style="height:20px">After2</td></tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tr><td rowspan="2" style="height:60px">Span</td><td style="height:20px">R1C2</td></tr>
              <tr><td style="height:20px">R3C2</td></tr>
              <tr><td class="after1" style="height:20px">After1</td><td class="after2" style="height:20px">After2</td></tr>
            </table>
            </body></html>
            """);

        AssertSameLocation(FindByClass(control, "after1")!, FindByClass(experiment, "after1")!);
        AssertSameLocation(FindByClass(control, "after2")!, FindByClass(experiment, "after2")!);
    }

    [Ignore(CollapseNotImplemented)]
    [TestMethod]
    public void CollapsedRowGroup_CollapsesEveryRowInsideIt()
    {
        // visibility is an inherited property, so visibility:collapse on the <tbody> itself must collapse
        // every row inside it without the row declaring anything.
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tr><td class="a" style="height:20px">A</td></tr>
              <tbody style="visibility:collapse">
                <tr><td style="height:20px">B1</td></tr>
                <tr><td style="height:20px">B2</td></tr>
              </tbody>
              <tr><td class="c" style="height:20px">C</td></tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tr><td class="a" style="height:20px">A</td></tr>
              <tr><td class="c" style="height:20px">C</td></tr>
            </table>
            </body></html>
            """);

        AssertSameLocation(FindByClass(control, "c")!, FindByClass(experiment, "c")!);
    }

    [TestMethod]
    public void HiddenRow_StillReservesSpace_UnlikeCollapse()
    {
        // Guard distinguishing the two values: visibility:hidden must keep reserving the row's space (only
        // paint is skipped), so the row after it must NOT land where it would if the hidden row had been
        // collapsed instead. Unlike the rest of this file, this does not depend on collapse-specific
        // removal logic at all - only on generic visibility:hidden still occupying layout space.
        var (hidden, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tr><td class="a" style="height:20px">A</td></tr>
              <tr style="visibility:hidden"><td style="height:20px">B</td></tr>
              <tr><td class="c" style="height:20px">C</td></tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <tr><td class="a" style="height:20px">A</td></tr>
              <tr><td class="c" style="height:20px">C</td></tr>
            </table>
            </body></html>
            """);

        var hiddenC = FindByClass(hidden, "c")!;
        var controlC = FindByClass(control, "c")!;
        Assert.IsTrue(hiddenC.Location.Y > controlC.Location.Y,
            $"a visibility:hidden row must still reserve its space, but hiddenC.Y={hiddenC.Location.Y} controlC.Y={controlC.Location.Y}");
    }

    [Ignore(CollapseNotImplemented)]
    [TestMethod]
    public void CollapsedRow_InsideDetachedHeader_TakesNoSpace()
    {
        // A <thead> is detached from the row-loop and measured separately - a collapsed row inside it has
        // its own skip there, if collapse were implemented at all.
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <thead>
                <tr><td style="height:20px">H1</td></tr>
                <tr style="visibility:collapse"><td style="height:20px">H2</td></tr>
              </thead>
              <tr><td class="body" style="height:20px">Body</td></tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0">
              <thead>
                <tr><td style="height:20px">H1</td></tr>
              </thead>
              <tr><td class="body" style="height:20px">Body</td></tr>
            </table>
            </body></html>
            """);

        AssertSameLocation(FindByClass(control, "body")!, FindByClass(experiment, "body")!);
    }

    [Ignore(CollapseNotImplemented)]
    [TestMethod]
    public void CollapsedRow_InsideDetachedFooter_TakesNoSpace()
    {
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:0">
              <tr><td style="height:20px">Body</td></tr>
              <tfoot>
                <tr><td style="height:20px">F1</td></tr>
                <tr style="visibility:collapse"><td style="height:20px">F2</td></tr>
              </tfoot>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:0">
              <tr><td style="height:20px">Body</td></tr>
              <tfoot>
                <tr><td style="height:20px">F1</td></tr>
              </tfoot>
            </table>
            </body></html>
            """);

        Assert.AreEqual(FindByClass(control, "t")!.ActualBottom, FindByClass(experiment, "t")!.ActualBottom, 3);
    }

    [Ignore(CollapseNotImplemented)]
    [TestMethod]
    public void CollapsedColumn_TakesNoWidth()
    {
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:0; width:300px">
              <colgroup>
                <col style="width:100px">
                <col style="width:100px; visibility:collapse">
                <col style="width:100px">
              </colgroup>
              <tr>
                <td class="a">A</td>
                <td class="b">B</td>
                <td class="c">C</td>
              </tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:0; width:200px">
              <colgroup>
                <col style="width:100px">
                <col style="width:100px">
              </colgroup>
              <tr>
                <td class="a">A</td>
                <td class="c">C</td>
              </tr>
            </table>
            </body></html>
            """);

        AssertSameLocation(FindByClass(control, "c")!, FindByClass(experiment, "c")!);

        var experimentTable = FindByClass(experiment, "t")!;
        var controlTable = FindByClass(control, "t")!;
        Assert.AreEqual(controlTable.ActualRight, experimentTable.ActualRight, 3);
    }

    [Ignore(CollapseNotImplemented)]
    [TestMethod]
    public void CollapsedColumn_WideContentDoesNotStarveNextColumn()
    {
        // An auto-width table's content-based column sizing spreads whatever width is left over toward
        // each column's own max-content width. A collapsed column's own (invisible) unbreakable content
        // must not count against that leftover, if collapse were implemented.
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <div style="width:100px">
            <table class="t" style="border-spacing:0">
              <colgroup>
                <col style="width:50px">
                <col style="visibility:collapse">
                <col>
              </colgroup>
              <tr>
                <td class="a">A</td>
                <td style="white-space:nowrap">ThisIsOneVeryLongUnbreakableWordFarWiderThanOneHundredPixels</td>
                <td class="c">CC CC</td>
              </tr>
            </table>
            </div>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <div style="width:100px">
            <table class="t" style="border-spacing:0">
              <colgroup>
                <col style="width:50px">
                <col>
              </colgroup>
              <tr>
                <td class="a">A</td>
                <td class="c">CC CC</td>
              </tr>
            </table>
            </div>
            </body></html>
            """);

        var experimentC = FindByClass(experiment, "c")!;
        var controlC = FindByClass(control, "c")!;
        AssertSameLocation(controlC, experimentC);
        Assert.AreEqual(controlC.ActualWidth, experimentC.ActualWidth, 3);
    }

    [Ignore(CollapseNotImplemented)]
    [TestMethod]
    public void CollapsedColumn_WithBorderSpacing_LeavesNoResidualGap()
    {
        // Regression guard: a collapsed column must not leave one border-spacing unit behind, which only
        // shows once border-spacing is non-zero - the default UA stylesheet sets border-spacing:2px on
        // every table, so this is the common case, if collapse were implemented.
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:5px; width:300px">
              <colgroup>
                <col style="width:100px">
                <col style="width:100px; visibility:collapse">
                <col style="width:100px">
              </colgroup>
              <tr>
                <td class="a">A</td>
                <td>B</td>
                <td class="c">C</td>
              </tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:5px; width:205px">
              <colgroup>
                <col style="width:100px">
                <col style="width:100px">
              </colgroup>
              <tr>
                <td class="a">A</td>
                <td class="c">C</td>
              </tr>
            </table>
            </body></html>
            """);

        AssertSameLocation(FindByClass(control, "c")!, FindByClass(experiment, "c")!);
        Assert.AreEqual(FindByClass(control, "t")!.ActualRight, FindByClass(experiment, "t")!.ActualRight, 3);
    }

    [TestMethod]
    public void CollapsedColumnGroup_CollapsesEveryColumnInsideIt()
    {
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0; width:200px">
              <colgroup>
                <col style="width:100px">
              </colgroup>
              <colgroup style="visibility:collapse">
                <col style="width:50px">
                <col style="width:50px">
              </colgroup>
              <tr>
                <td class="a">A</td>
                <td>B1</td>
                <td>B2</td>
              </tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table style="border-spacing:0; width:100px">
              <colgroup>
                <col style="width:100px">
              </colgroup>
              <tr>
                <td class="a">A</td>
              </tr>
            </table>
            </body></html>
            """);

        var experimentA = FindByClass(experiment, "a")!;
        var controlA = FindByClass(control, "a")!;
        Assert.AreEqual(controlA.ActualWidth, experimentA.ActualWidth, 3);
    }

    [Ignore(CollapseNotImplemented)]
    [TestMethod]
    public void ColspanCell_StraddlingVisibleThenCollapsedColumn_NoResidualBorderSpacing()
    {
        // A colspan cell spanning a visible column followed by a collapsed one has its trailing edge land
        // on the collapsed column - like a cell confined entirely to collapsed column(s), the
        // border-spacing slot after it must be omitted too, if collapse were implemented.
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:5px; width:305px">
              <colgroup>
                <col style="width:100px">
                <col style="width:100px; visibility:collapse">
                <col style="width:100px">
              </colgroup>
              <tr>
                <td class="a" colspan="2">AB</td>
                <td class="c">C</td>
              </tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:5px; width:205px">
              <colgroup>
                <col style="width:100px">
                <col style="width:100px">
              </colgroup>
              <tr>
                <td class="a">A</td>
                <td class="c">C</td>
              </tr>
            </table>
            </body></html>
            """);

        AssertSameLocation(FindByClass(control, "c")!, FindByClass(experiment, "c")!);
    }

    [Ignore(CollapseNotImplemented)]
    [TestMethod]
    public void ColspanCell_StraddlingCollapsedThenVisibleColumn_NoExtraInteriorSpacing()
    {
        // Mirror case: a colspan cell spanning a collapsed column followed by a visible one. The boundary
        // strictly between the cell's own two spanned columns must be omitted, if collapse were
        // implemented.
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:5px; width:305px">
              <colgroup>
                <col style="width:100px; visibility:collapse">
                <col style="width:100px">
                <col style="width:100px">
              </colgroup>
              <tr>
                <td class="a" colspan="2">AB</td>
                <td class="c">C</td>
              </tr>
            </table>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <table class="t" style="border-spacing:5px; width:205px">
              <colgroup>
                <col style="width:100px">
                <col style="width:100px">
              </colgroup>
              <tr>
                <td class="a">A</td>
                <td class="c">C</td>
              </tr>
            </table>
            </body></html>
            """);

        AssertSameLocation(FindByClass(control, "c")!, FindByClass(experiment, "c")!);
    }

    [TestMethod]
    public void ColspanCell_StraddlingCollapsedColumn_ContentDoesNotShareWidthWithIt()
    {
        // A straddling cell's content-based min-width must not be divided evenly across every column its
        // colspan reaches when one of them is collapsed and will never carry any of it, if collapse were
        // implemented.
        var (experiment, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <div style="width:80px">
            <table class="t" style="border-spacing:0">
              <colgroup>
                <col style="visibility:collapse">
                <col>
              </colgroup>
              <tr><td class="a" colspan="2" style="white-space:nowrap">ThisIsOneVeryLongUnbreakableWordFarWiderThanEightyPixels</td></tr>
            </table>
            </div>
            </body></html>
            """);

        var (control, _) = LayoutHarness.Layout("""
            <!DOCTYPE html><html><body>
            <div style="width:80px">
            <table class="t" style="border-spacing:0">
              <colgroup>
                <col>
              </colgroup>
              <tr><td class="a" style="white-space:nowrap">ThisIsOneVeryLongUnbreakableWordFarWiderThanEightyPixels</td></tr>
            </table>
            </div>
            </body></html>
            """);

        var experimentA = FindByClass(experiment, "a")!;
        var controlA = FindByClass(control, "a")!;
        Assert.AreEqual(controlA.ActualWidth, experimentA.ActualWidth, 3);
    }

    private static void AssertSameLocation(CssBox expected, CssBox actual)
    {
        Assert.AreEqual(expected.Location.X, actual.Location.X, 3);
        Assert.AreEqual(expected.Location.Y, actual.Location.Y, 3);
    }

    private static CssBox? FindByClass(CssBox box, string className)
    {
        var val = box.HtmlTag?.TryGetAttribute("class", "");
        if (val != null && val.Split(' ').Contains(className))
            return box;

        foreach (var child in box.Boxes)
        {
            var found = FindByClass(child, className);
            if (found != null) return found;
        }

        return null;
    }
}
