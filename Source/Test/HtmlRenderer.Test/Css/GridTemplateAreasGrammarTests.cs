using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/GridTemplateAreasGrammarTests.cs.
/// Tests for the shared <see cref="GridTemplateAreasGrammar"/> - the <c>grid-template-areas</c> value
/// (a rectangular grid of named cells), confirmed a field-for-field match against PeachPDF's
/// implementation.
/// </summary>
[TestClass]
public sealed class GridTemplateAreasGrammarTests
{
    private static GridAreas? Parse(string value) =>
        GridTemplateAreasGrammar.TryParse(CssValueParser.GetCssTokens(value));

    [TestMethod]
    public void RectangularGrid_ParsesWithAreaBounds()
    {
        var areas = Parse("\"header header header\" \"nav main main\" \"footer footer footer\"");
        Assert.IsNotNull(areas);
        Assert.AreEqual(3, areas!.RowCount);
        Assert.AreEqual(3, areas.ColCount);
        Assert.AreEqual((0, 0, 0, 2), areas.Areas["header"]);   // whole top row
        Assert.AreEqual((1, 0, 1, 0), areas.Areas["nav"]);      // row 1, col 0
        Assert.AreEqual((1, 1, 1, 2), areas.Areas["main"]);     // row 1, cols 1-2
        Assert.AreEqual((2, 0, 2, 2), areas.Areas["footer"]);
    }

    [TestMethod]
    public void DotCells_AreEmpty_AndNotAreas()
    {
        var areas = Parse("\"a . b\" \". . .\"");
        Assert.IsNotNull(areas);
        Assert.AreEqual(2, areas!.RowCount);
        Assert.AreEqual(3, areas.ColCount);
        Assert.IsTrue(areas.Areas.ContainsKey("a"));
        Assert.IsTrue(areas.Areas.ContainsKey("b"));
        Assert.IsNull(areas.Cells[0, 1]);
        Assert.IsNull(areas.Cells[1, 0]);
    }

    [TestMethod]
    public void TripleDot_IsAlsoAnEmptyCell()
    {
        var areas = Parse("\"a ... b\"");
        Assert.IsNotNull(areas);
        Assert.IsNull(areas!.Cells[0, 1]);
    }

    [TestMethod]
    [DataRow("\"a a\" \"a a a\"")]        // ragged rows (2 vs 3 columns)
    [DataRow("\"a b\" \"b a\"")]          // 'a' is not a rectangle (diagonal)
    [DataRow("\"a a\" \"a .\"")]          // 'a' bounding box includes an empty cell → not filled
    [DataRow("\"a b a\"")]                // 'a' occupies cols 0 and 2 with b between → not rectangular
    [DataRow("100px")]                    // not a string list
    [DataRow("\"\"")]                     // an empty string row
    [DataRow("\"a.b c\"")]                // a cell mixing a name and a dot is not a valid cell token
    [DataRow("")]
    public void Invalid_ReturnsNull(string value)
    {
        Assert.IsNull(Parse(value));
    }
}
