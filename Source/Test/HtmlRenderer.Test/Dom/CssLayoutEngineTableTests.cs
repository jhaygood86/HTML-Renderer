using System.Linq;
using HtmlRenderer.Test.TestSupport;

namespace HtmlRenderer.Test.Dom;

/// <summary>
/// Basic table layout tests against <see cref="TheArtOfDev.HtmlRenderer.Core.Dom.CssLayoutEngineTable"/>,
/// ported from PeachPDF's (much larger) CssLayoutEngineTableTests. HTML-Renderer's table engine is a
/// simpler fork ancestor -- it has no header/footer repeat-across-pages proxies, no caption
/// grid-decoration box, and (unlike PeachPDF's <c>CssBox.Display</c> enum) represents "Display" as a
/// plain string compared against <see cref="TheArtOfDev.HtmlRenderer.Core.Utils.CssConstants"/>. Only the parts of the original suite that
/// exercise basic dimension/colspan/rowspan layout -- functionality this engine actually has -- are
/// ported here.
/// </summary>
[TestClass]
public sealed class CssLayoutEngineTableTests
{
    [TestMethod]
    public void TableLayout_CalculatesCorrectDimensions()
    {
        var html = LayoutHarness.Wrap(
            "<table id='tbl' style='width:100%;border-collapse:collapse'>" +
            "<tr><td style='border:1px solid black;padding:8px'>Cell 1</td><td style='border:1px solid black;padding:8px'>Cell 2</td></tr>" +
            "<tr><td style='border:1px solid black;padding:8px'>Cell 3</td><td style='border:1px solid black;padding:8px'>Cell 4</td></tr>" +
            "</table>");

        var (root, _) = LayoutHarness.Layout(html);
        var table = LayoutHarness.FindById(root, "tbl");

        Assert.IsNotNull(table);
        Assert.IsTrue(table!.ActualRight > table.Location.X, "Table should have width");
        Assert.IsTrue(table.ActualBottom > table.Location.Y, "Table should have height");
    }

    [TestMethod]
    public void TableLayout_WithColspan_CalculatesCorrectWidth()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:100%;border-collapse:collapse'>" +
            "<tr>" +
            "<td id='wide' colspan='2' style='border:1px solid black;padding:8px'>Wide Cell</td>" +
            "<td id='normal' style='border:1px solid black;padding:8px'>Normal</td>" +
            "</tr>" +
            "<tr>" +
            "<td style='border:1px solid black;padding:8px'>Cell 1</td>" +
            "<td style='border:1px solid black;padding:8px'>Cell 2</td>" +
            "<td style='border:1px solid black;padding:8px'>Cell 3</td>" +
            "</tr>" +
            "</table>");

        var (root, _) = LayoutHarness.Layout(html);
        var wideCell = LayoutHarness.FindById(root, "wide");
        var normalCell = LayoutHarness.FindById(root, "normal");

        Assert.IsNotNull(wideCell);
        Assert.IsNotNull(normalCell);

        var wideWidth = wideCell!.ActualRight - wideCell.Location.X;
        var normalWidth = normalCell!.ActualRight - normalCell.Location.X;

        Assert.IsTrue(wideWidth > normalWidth, "Colspan cell should be wider than single cell");
    }

    [TestMethod]
    public void TableLayout_WithRowspan_CalculatesCorrectHeight()
    {
        var html = LayoutHarness.Wrap(
            "<table style='border-collapse:collapse'>" +
            "<tr><td id='tall' rowspan='2' style='border:1px solid black;padding:8px'>Tall Cell</td><td style='border:1px solid black;padding:8px'>Cell 2</td></tr>" +
            "<tr><td style='border:1px solid black;padding:8px'>Cell 3</td></tr>" +
            "<tr><td style='border:1px solid black;padding:8px'>Cell 4</td><td style='border:1px solid black;padding:8px'>Cell 5</td></tr>" +
            "</table>");

        var (root, _) = LayoutHarness.Layout(html);
        var tallCell = LayoutHarness.FindById(root, "tall");

        Assert.IsNotNull(tallCell);
        var tallCellHeight = tallCell!.ActualBottom - tallCell.Location.Y;
        Assert.IsTrue(tallCellHeight > 0, "Rowspan cell should have height");
    }

    [TestMethod]
    public void TableLayout_DistributesWidthEqually_WhenNoWidthsSpecified()
    {
        var html = LayoutHarness.Wrap(
            "<table style='width:600px;border-collapse:collapse'>" +
            "<tr>" +
            "<td id='c1' style='border:1px solid black;padding:8px'>Cell 1</td>" +
            "<td id='c2' style='border:1px solid black;padding:8px'>Cell 2</td>" +
            "<td id='c3' style='border:1px solid black;padding:8px'>Cell 3</td>" +
            "</tr>" +
            "</table>");

        var (root, _) = LayoutHarness.Layout(html, maxWidth: 1200);
        var cells = new[] { "c1", "c2", "c3" }
            .Select(id => LayoutHarness.FindById(root, id))
            .ToList();

        Assert.IsTrue(cells.All(c => c is not null));

        var widths = cells.Select(c => c!.ActualRight - c.Location.X).ToList();
        var avgWidth = widths.Average();

        foreach (var width in widths)
        {
            Assert.IsTrue(System.Math.Abs(width - avgWidth) < 5,
                $"Cell width {width} should be close to average {avgWidth}");
        }
    }

    [TestMethod]
    public void TableLayout_RespectsSpecifiedColumnWidths()
    {
        // Adapted from the source test: HTML-Renderer's CssParser has no pseudo-class selector support
        // (no ":first-child"), so the explicit width is applied directly on the first cell via an
        // inline style rather than through a "td:first-child { width: ... }" rule.
        var html = LayoutHarness.Wrap(
            "<table style='width:600pt;border-collapse:collapse'>" +
            "<tr>" +
            "<td id='wide' style='border:1px solid black;padding:8px;width:200pt'>Wide Cell</td>" +
            "<td id='auto1' style='border:1px solid black;padding:8px'>Auto</td>" +
            "<td id='auto2' style='border:1px solid black;padding:8px'>Auto</td>" +
            "<td id='auto3' style='border:1px solid black;padding:8px'>Auto</td>" +
            "</tr>" +
            "</table>");

        var (root, _) = LayoutHarness.Layout(html, maxWidth: 1200);
        var wideCell = LayoutHarness.FindById(root, "wide");
        var auto1Cell = LayoutHarness.FindById(root, "auto1");

        Assert.IsNotNull(wideCell);
        Assert.IsNotNull(auto1Cell);

        var wideWidth = wideCell!.ActualRight - wideCell.Location.X;
        var auto1Width = auto1Cell!.ActualRight - auto1Cell.Location.X;

        Assert.IsTrue(wideWidth >= 180, $"First cell should be approximately 200px wide (accounting for borders), but was {wideWidth}");
        // The remaining table width (600pt minus the 200pt explicit column) is distributed across the
        // three auto columns - via their content-based max width plus an equal share of the leftover
        // space (see CssLayoutEngineTable.DetermineMissingColumnWidths) - rather than split evenly by a
        // fixed pixel budget, so assert the semantically-intended relationship (narrower than the
        // explicitly-widened column) instead of an arbitrary absolute threshold.
        Assert.IsTrue(auto1Width < wideWidth, $"Auto cell ({auto1Width}) should be narrower than the explicitly-widened first cell ({wideWidth})");
    }
}
