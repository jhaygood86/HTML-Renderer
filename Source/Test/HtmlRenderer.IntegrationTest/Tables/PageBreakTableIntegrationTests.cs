using System;
using System.Text;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Tables;

/// <summary>
/// Verifies table page-break behaviour.
/// </summary>
/// <remarks>
/// HTML-Renderer fact (confirmed, <c>Dom\CssLayoutEngineTable.cs</c>): the ONLY page-break-related check in
/// the table layout engine is <c>if (_tableBox.PageBreakInside == CssConstants.Avoid)</c> - the TABLE's own
/// <c>page-break-inside</c> property, checked once per row, gating a call to <c>CssBox.BreakPage()</c> for
/// each cell in that row. There is no automatic/implicit avoidance (PeachPDF assumes automatic avoidance,
/// matching how browsers try to avoid breaking table rows by default) - this fork requires an explicit
/// <c>page-break-inside:avoid</c> declared directly on the &lt;table&gt; element to get ANY avoidance
/// behaviour at all. There is also no pre-layout height ESTIMATE, no POST-layout whole-table relocation
/// pass, and no keep-with-next/break-after handling for headings - <c>CssBox.BreakPage()</c>, invoked
/// inline during normal row layout, is the entire mechanism. Cases assuming automatic avoidance or any of
/// those extra passes are [Ignore]d; cases whose expected outcome holds regardless (e.g. "not moved", which
/// is also just this fork's default with no page-break-inside declared at all) are left active.
/// </remarks>
[DoNotParallelize]
[TestClass]
public sealed class PageBreakTableIntegrationTests
{
    // Mirrors PeachPDF's A4-at-1:1-scale page height; treated as px here since LayoutHarness measures in
    // px, not pt - the numbers below only need generous relative separation, which they already have.
    private const double PageHeight = 842.0;

    // A spacer this tall pushes the table close enough to the page end to plausibly cross a page boundary
    // for a typical single-row table height, leaving only a small margin.
    private const double SpacerThatCrossesPage = 833;

    // A spacer this tall leaves plenty of room - no page-break needed under any mechanism.
    private const double SpacerThatFits = 200;

    [Ignore("Assumes automatic/implicit page-break avoidance (no page-break-inside:avoid declared on the " +
            "<table>) - this fork's CssLayoutEngineTable only ever calls CssBox.BreakPage() when the " +
            "table's OWN page-break-inside is explicitly 'avoid'; without it, a single-row table straddling " +
            "the page boundary is never relocated.")]
    [TestMethod]
    public void SingleRowTable_CrossingPageBoundary_IsMovedToNextPage()
    {
        var html = BuildHtml(SpacerThatCrossesPage, rowCount: 1);
        var (table, _) = GetTableAndPageHeight(html);

        Assert.IsNotNull(table);
        Assert.IsTrue(table!.Location.Y >= PageHeight,
            $"Single-row table should be on page 2 (Y >= {PageHeight}) but Y={table.Location.Y:F1}");
    }

    [TestMethod]
    public void SingleRowTable_FitsOnCurrentPage_IsNotMoved()
    {
        var html = BuildHtml(SpacerThatFits, rowCount: 1);
        var (table, _) = GetTableAndPageHeight(html);

        Assert.IsNotNull(table);
        Assert.IsTrue(table!.Location.Y < PageHeight,
            $"Table that fits should stay on page 1 (Y < {PageHeight}) but Y={table.Location.Y:F1}");
    }

    [TestMethod]
    public void MultiRowTable_CrossingPageBoundary_PerRowBreakStillWorks()
    {
        // PeachPDF's original ran a full PDF-generation pass and asserted no exception. This fork has no
        // PdfGenerator/PDF-generation API at all (it is a WinForms/GDI+ HTML renderer, not a PDF library),
        // so this is adapted into a layout-only smoke test: a multi-row table near the page boundary must
        // still lay out without throwing, even though (per the class remarks) no automatic per-row
        // page-break relocation happens here without an explicit page-break-inside:avoid on the table.
        var html = BuildHtml(SpacerThatCrossesPage, rowCount: 3);

        Exception? thrown = null;
        try
        {
            LayoutHarness.Layout(html, 595, PageHeight);
        }
        catch (Exception ex)
        {
            thrown = ex;
        }

        Assert.IsNull(thrown, $"Multi-row table layout near a page boundary should not throw, but got: {thrown}");
    }

    [TestMethod]
    public void SingleRowTable_WithRoundedBoxes_GeneratesPdf()
    {
        // Adapted from a PDF-generation regression smoke test (PdfGenerator.GeneratePdf does not exist in
        // this fork) into a layout-only smoke test: a page of single-row tables with border-radius content
        // near a page boundary must lay out without throwing. Trimmed to two of the original six sections
        // (including the "Combined Styles" one specifically called out as the original regression trigger)
        // to keep this focused; the @page at-rule was dropped since this fork's parser is not known to
        // support it.
        const string html = """
            <!DOCTYPE html><html><head><style>
            body { font: 8.5px Arial, sans-serif; margin: 0 }
            h2 { font-size: 10px; margin: 0.9em 0 0.3em; padding-bottom: 2px; border-bottom: 1px solid #999 }
            table.sw { border-collapse: collapse; width: 100%; margin-bottom: 0.3em }
            table.sw td { padding: 3px; vertical-align: top; width: 25% }
            .rbox { height: 60px; background: steelblue; border: 2px solid #1a6b8a; margin-bottom: 3px }
            .desc { font-size: 7px; font-weight: bold; color: #444; margin-bottom: 1px }
            .css  { font-size: 6px; color: #666; line-height: 1.3; word-break: break-all }
            </style></head><body>
            <h2>1</h2><table class="sw"><tr>
              <td><div class="rbox" style="border-radius:20px"></div><div class="desc">a</div><div class="css">a</div></td>
              <td><div class="rbox" style="border-radius:10px 30px"></div><div class="desc">b</div><div class="css">b</div></td>
              <td><div class="rbox" style="border-radius:8px 20px 35px"></div><div class="desc">c</div><div class="css">c</div></td>
              <td><div class="rbox" style="border-radius:5px 15px 30px 45px"></div><div class="desc">d</div><div class="css">d</div></td>
            </tr></table>
            <h2>6 - Combined Styles</h2><table class="sw"><tr>
              <td><div class="rbox" style="border-radius:15px"></div><div class="desc">solid border + bg</div><div class="css">border-radius: 15px</div></td>
              <td><div class="rbox" style="border-style:dashed;border-radius:15px"></div><div class="desc">dashed border</div><div class="css">border-radius: 15px</div></td>
              <td><div class="rbox" style="border-style:dotted;border-radius:15px"></div><div class="desc">dotted border</div><div class="css">border-radius: 15px</div></td>
              <td><div class="rbox" style="border:none;border-radius:15px"></div><div class="desc">no border, bg only</div><div class="css">border-radius: 15px</div></td>
            </tr></table>
            </body></html>
            """;

        Exception? thrown = null;
        try
        {
            LayoutHarness.Layout(html, 595, PageHeight * 3);
        }
        catch (Exception ex)
        {
            thrown = ex;
        }

        Assert.IsNull(thrown, $"Layout of tables with border-radius content should not throw, but got: {thrown}");
    }

    [Ignore("Relies on PeachPDF's pre-layout height ESTIMATE missing tall cell content, followed by a " +
            "POST-layout correction pass that relocates the table once the real straddle is discovered. " +
            "This fork has neither an estimate nor a post-layout correction pass - CssBox.BreakPage() is " +
            "only checked inline during row layout, and only when the table declares " +
            "page-break-inside:avoid (not the case here), so tall cell content that straddles the boundary " +
            "is never relocated.")]
    [TestMethod]
    public void SingleRowTable_TallCellContentMissedByEstimate_IsMovedToNextPageAfterLayout()
    {
        var html = BuildTallContentHtml(spacerHeight: 500, contentHeight: 400);
        var (table, pageHeight) = GetTableAndPageHeight(html);

        Assert.IsNotNull(table);
        Assert.IsTrue(table!.Location.Y >= PageHeight,
            $"Table with tall cell content should be moved to page 2 (Y >= {PageHeight}) but Y={table.Location.Y:F1}");
        Assert.AreEqual(PageHeight, table.Location.Y, 1.0,
            $"Moved table should start flush at the next page top ({PageHeight}) but Y={table.Location.Y:F1}");
        Assert.IsTrue(table.ActualBottom - table.Location.Y <= pageHeight,
            "Moved table must fit within a single page");
    }

    [TestMethod]
    public void SingleRowTable_TallerThanOnePage_IsLeftInPlace()
    {
        // An unsatisfiable move: the row is taller than a whole page. Holds here for a different reason
        // than in PeachPDF - this fork never relocates the table automatically at all (no
        // page-break-inside declared), so it trivially stays in place; even opting into avoidance
        // wouldn't change the outcome, since CssBox.BreakPage() itself declines to move a box whose own
        // height already exceeds the page height.
        var html = BuildTallContentHtml(spacerHeight: 500, contentHeight: 900);
        var (table, _) = GetTableAndPageHeight(html);

        Assert.IsNotNull(table);
        Assert.IsTrue(table!.Location.Y < PageHeight,
            $"Table taller than a page should stay on page 1 (Y < {PageHeight}) but Y={table.Location.Y:F1}");
    }

    [Ignore("Relies on a POST-layout whole-table 'move' pass honoring css-break keep-with-next (the UA " +
            "default h1-h6 { break-after: avoid } under print media) to pull a preceding heading along " +
            "with a relocated table. This fork implements neither the post-layout relocation pass nor any " +
            "break-after/keep-with-next handling for headings.")]
    [TestMethod]
    public void SingleRowTable_MovedByPostCheck_PullsAvoidChainedHeadingAlong()
    {
        const string html = """
            <!DOCTYPE html><html><head><style>
            body { margin: 0; }
            h2 { margin: 6px 0; }
            table { border-collapse: collapse; width: 100%; }
            td { padding: 3px; }
            </style></head><body>
            <div style='height: 500px'></div>
            <h2 class='heading'>Section heading</h2>
            <table><tr><td><div style='height: 400px'>tall content</div></td></tr></table>
            </body></html>
            """;

        var (root, _) = LayoutHarness.Layout(html, 595, PageHeight);

        var table = FindFirst(root, b => b.Display == "table");
        var heading = FindFirst(root, b => b.HtmlTag?.Name == "h2");
        Assert.IsNotNull(table);
        Assert.IsNotNull(heading);

        Assert.IsTrue(table!.Location.Y >= PageHeight,
            $"Test setup expects the table to be moved to page 2 (Y >= {PageHeight}) but Y={table.Location.Y:F1}");
        Assert.AreEqual(Math.Floor(table.Location.Y / PageHeight), Math.Floor(heading!.Location.Y / PageHeight));
        Assert.IsTrue(heading.ActualBottom <= table.Location.Y + 1.0,
            $"Heading (bottom={heading.ActualBottom:F1}) must sit above the moved table (top={table.Location.Y:F1})");
    }

    // A fixed-position box renders at the same page-box position on every page (CSS2.1 §13.3.1) - flow
    // pagination must never relocate it, even when its laid-out bounds straddle a page boundary. Same for
    // absolute positioning (§9.6). This holds in this fork trivially: with no page-break-inside declared on
    // the table at all, nothing is ever moved automatically regardless of position.
    [TestMethod]
    [DataRow("fixed")]
    [DataRow("absolute")]
    public void OutOfFlowTable_StraddlingPageBoundary_IsNotMoved(string position)
    {
        var html = $$"""
            <!DOCTYPE html><html><head><style>
            body { margin: 0; }
            table { border-collapse: collapse; width: 50%; position: {{position}}; top: 700px; }
            td { padding: 3px; }
            </style></head><body>
            <div style='height: 30px'>flow content</div>
            <table><tr><td><div style='height: 400px'>tall content</div></td></tr></table>
            </body></html>
            """;

        var (table, _) = GetTableAndPageHeight(html);

        Assert.IsNotNull(table);
        Assert.IsTrue(table!.Location.Y < PageHeight,
            $"A position: {position} table must not be relocated by page-break handling (Y={table.Location.Y:F1})");
    }

    // --- Helpers ---

    private static string BuildTallContentHtml(double spacerHeight, double contentHeight)
    {
        return $$"""
            <!DOCTYPE html><html><head><style>
            body { margin: 0; }
            table { border-collapse: collapse; width: 100%; }
            td { padding: 3px; }
            </style></head><body>
            <div style='height: {{spacerHeight}}px'></div>
            <table><tr><td><div style='height: {{contentHeight}}px'>tall content</div></td></tr></table>
            </body></html>
            """;
    }

    private static string BuildHtml(double spacerHeight, int rowCount)
    {
        var rows = new StringBuilder();
        for (var r = 0; r < rowCount; r++)
        {
            rows.Append("<tr>");
            rows.Append("<td><div class='rbox'></div></td>");
            rows.Append("<td><div class='rbox'></div></td>");
            rows.Append("</tr>");
        }

        return $$"""
            <!DOCTYPE html><html><head><style>
            body { margin: 0; }
            .spacer { height: {{spacerHeight}}px; }
            table { border-collapse: collapse; width: 100%; }
            td { padding: 3px; }
            .rbox { height: 60px; }
            </style></head><body>
            <div class='spacer'></div>
            <table>{{rows}}</table>
            </body></html>
            """;
    }

    private static (CssBox? table, double pageHeight) GetTableAndPageHeight(string html)
    {
        var (root, container) = LayoutHarness.Layout(html, 595, PageHeight);
        var table = FindFirst(root, b => b.Display == "table");
        return (table, container.PageSize.Height);
    }

    private static CssBox? FindFirst(CssBox box, Func<CssBox, bool> predicate)
    {
        if (predicate(box)) return box;
        foreach (var child in box.Boxes)
        {
            var found = FindFirst(child, predicate);
            if (found != null) return found;
        }
        return null;
    }
}
