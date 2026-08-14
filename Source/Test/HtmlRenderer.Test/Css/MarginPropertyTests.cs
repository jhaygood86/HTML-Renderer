using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/MarginProperty.cs.
/// Only the 1-4 value `margin` shorthand-splitting cases apply. The individual longhand-property
/// tests (margin-left, margin-right, margin-top, margin-bottom) and the CSS text
/// recombination/simplification tests (MarginShouldBeRecombinedCorrectly and friends, which depend on
/// PeachPDF's own CSSOM (its <c>PeachPDF.CSS</c> engine, a fork of ExCSS) re-serializing a rule back to text) have no equivalent surface
/// in HTML-Renderer and were dropped.
/// Shorthand splitting is exercised through the real parsing pipeline (CssParser.ParseCssBlock -&gt;
/// AddProperty -&gt; ParseMarginProperty -&gt; SplitMultiDirectionValues), the same code path inline
/// "style" attributes and stylesheet rules go through. Per SplitMultiDirectionValues: 1 value applies
/// to all sides, 2 values are (vertical, horizontal), 3 values are (top, horizontal, bottom), 4 values
/// are (top, right, bottom, left), and anything outside 1-4 values leaves all four longhand properties
/// unset.
/// </summary>
[TestClass]
public sealed class MarginPropertyTests
{
    private static IDictionary<string, string> ParseProperties(string declaration)
    {
        var parser = new CssParser(new MockAdapter());
        var block = parser.ParseCssBlock("test", declaration);
        Assert.IsNotNull(block);
        return block.Properties;
    }

    [TestMethod]
    public void MarginAllZeroLegal()
    {
        var properties = ParseProperties("margin: 0");

        Assert.AreEqual("0", properties["margin-left"]);
        Assert.AreEqual("0", properties["margin-top"]);
        Assert.AreEqual("0", properties["margin-right"]);
        Assert.AreEqual("0", properties["margin-bottom"]);
    }

    [TestMethod]
    public void MarginAllPercentLegal()
    {
        var properties = ParseProperties("margin: 25%");

        Assert.AreEqual("25%", properties["margin-left"]);
        Assert.AreEqual("25%", properties["margin-top"]);
        Assert.AreEqual("25%", properties["margin-right"]);
        Assert.AreEqual("25%", properties["margin-bottom"]);
    }

    [TestMethod]
    public void MarginAutoLegal()
    {
        var properties = ParseProperties("margin: auto");

        Assert.AreEqual("auto", properties["margin-left"]);
        Assert.AreEqual("auto", properties["margin-top"]);
        Assert.AreEqual("auto", properties["margin-right"]);
        Assert.AreEqual("auto", properties["margin-bottom"]);
    }

    [TestMethod]
    public void MarginSidesLengthLegal()
    {
        var properties = ParseProperties("margin: 10px 3em");

        Assert.AreEqual("3em", properties["margin-left"]);
        Assert.AreEqual("10px", properties["margin-top"]);
        Assert.AreEqual("3em", properties["margin-right"]);
        Assert.AreEqual("10px", properties["margin-bottom"]);
    }

    [TestMethod]
    public void MarginSidesLengthAndAutoLegal()
    {
        var properties = ParseProperties("margin: 10px auto");

        Assert.AreEqual("auto", properties["margin-left"]);
        Assert.AreEqual("10px", properties["margin-top"]);
        Assert.AreEqual("auto", properties["margin-right"]);
        Assert.AreEqual("10px", properties["margin-bottom"]);
    }

    [TestMethod]
    public void MarginThreeValuesLegal()
    {
        var properties = ParseProperties("margin: 10px 3em 5px");

        Assert.AreEqual("3em", properties["margin-left"]);
        Assert.AreEqual("10px", properties["margin-top"]);
        Assert.AreEqual("3em", properties["margin-right"]);
        Assert.AreEqual("5px", properties["margin-bottom"]);
    }

    [TestMethod]
    public void MarginAllValuesWithPercentAndAutoLegal()
    {
        var properties = ParseProperties("margin: 10px 5% auto 2%");

        Assert.AreEqual("2%", properties["margin-left"]);
        Assert.AreEqual("10px", properties["margin-top"]);
        Assert.AreEqual("5%", properties["margin-right"]);
        Assert.AreEqual("auto", properties["margin-bottom"]);
    }

    [TestMethod]
    public void MarginTooManyValuesIllegal()
    {
        var properties = ParseProperties("margin: 10px 5% 8px 2% 3px auto");

        Assert.IsFalse(properties.ContainsKey("margin-left"));
        Assert.IsFalse(properties.ContainsKey("margin-top"));
        Assert.IsFalse(properties.ContainsKey("margin-right"));
        Assert.IsFalse(properties.ContainsKey("margin-bottom"));
    }
}
