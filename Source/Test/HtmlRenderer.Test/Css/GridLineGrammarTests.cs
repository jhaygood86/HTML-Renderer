using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/GridLineGrammarTests.cs.
/// Tests for the shared <see cref="GridLineGrammar"/> - the grid <c>&lt;grid-line&gt;</c> value
/// (<c>auto | &lt;integer&gt; | span &lt;integer&gt;</c>), confirmed a field-for-field match against
/// PeachPDF's implementation.
/// </summary>
[TestClass]
public sealed class GridLineGrammarTests
{
    private static GridLine? Parse(string value) =>
        GridLineGrammar.TryParse(CssValueParser.GetCssTokens(value));

    [TestMethod]
    public void Auto_Parses()
    {
        var line = Parse("auto");
        Assert.IsNotNull(line);
        Assert.IsTrue(line!.IsAuto);
    }

    [TestMethod]
    [DataRow("1", 1)]
    [DataRow("3", 3)]
    [DataRow("-1", -1)]
    public void IntegerLine_Parses(string value, int expected)
    {
        var line = Parse(value);
        Assert.IsNotNull(line);
        Assert.IsFalse(line!.IsAuto);
        Assert.IsFalse(line.IsSpan);
        Assert.AreEqual(expected, line.Value);
    }

    [TestMethod]
    [DataRow("span 1", 1)]
    [DataRow("span 3", 3)]
    public void Span_Parses(string value, int expected)
    {
        var line = Parse(value);
        Assert.IsNotNull(line);
        Assert.IsTrue(line!.IsSpan);
        Assert.AreEqual(expected, line.Value);
    }

    [TestMethod]
    [DataRow("sidebar")]
    [DataRow("main-start")]
    public void NamedLine_Parses(string value)
    {
        var line = Parse(value);
        Assert.IsNotNull(line);
        Assert.IsFalse(line!.IsAuto);
        Assert.IsFalse(line.IsSpan);
        Assert.AreEqual(value, line.Name);
        Assert.AreEqual(1, line.Value);
    }

    [TestMethod]
    [DataRow("col 2", "col", 2)]
    [DataRow("2 col", "col", 2)]
    [DataRow("col -1", "col", -1)]
    public void NamedNthLine_Parses(string value, string name, int nth)
    {
        var line = Parse(value);
        Assert.IsNotNull(line);
        Assert.AreEqual(name, line!.Name);
        Assert.AreEqual(nth, line.Value);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("0")]           // line 0 is invalid
    [DataRow("span 0")]      // span must be >= 1
    [DataRow("span")]        // span needs a count
    [DataRow("span auto")]
    [DataRow("1.5")]         // not an integer
    [DataRow("[name]")]      // bracketed line names belong in a track list, not a <grid-line>
    [DataRow("none")]        // a CSS-wide/reserved keyword is not a custom-ident line name
    [DataRow("initial")]
    [DataRow("span foo")]    // span <custom-ident> is out of v1 scope
    public void Invalid_ReturnsNull(string value)
    {
        Assert.IsNull(Parse(value));
    }
}
