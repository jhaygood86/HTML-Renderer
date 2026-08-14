using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/PaddingProperty.cs.
/// Only the 1-4 value `padding` shorthand-splitting cases apply, mirroring MarginPropertyTests. The
/// individual longhand-property tests were dropped (out of scope per the porting brief), and so was
/// CssPaddingAutoIllegal: TheArtOfDev.HtmlRenderer.Core.Parse.CssParser's ParsePaddingProperty only
/// splits the shorthand value on whitespace (SplitMultiDirectionValues) with no keyword validation, so
/// "padding: auto" is not rejected here the way PeachPDF's ExCSS-fork-based CSS engine rejects it - the
/// literal token "auto" is simply carried through onto all four longhand properties like any other
/// value would be.
/// Shorthand splitting is exercised through the real parsing pipeline (CssParser.ParseCssBlock -&gt;
/// AddProperty -&gt; ParsePaddingProperty -&gt; SplitMultiDirectionValues), the same code path inline
/// "style" attributes and stylesheet rules go through.
/// </summary>
[TestClass]
public sealed class PaddingPropertyTests
{
    private static IDictionary<string, string> ParseProperties(string declaration)
    {
        var parser = new CssParser(new MockAdapter());
        var block = parser.ParseCssBlock("test", declaration);
        Assert.IsNotNull(block);
        return block.Properties;
    }

    [TestMethod]
    public void CssPaddingAllZeroLegal()
    {
        var properties = ParseProperties("padding: 0");

        Assert.AreEqual("0", properties["padding-left"]);
        Assert.AreEqual("0", properties["padding-top"]);
        Assert.AreEqual("0", properties["padding-right"]);
        Assert.AreEqual("0", properties["padding-bottom"]);
    }

    [TestMethod]
    public void CssPaddingAllPercentLegal()
    {
        var properties = ParseProperties("padding: 25%");

        Assert.AreEqual("25%", properties["padding-left"]);
        Assert.AreEqual("25%", properties["padding-top"]);
        Assert.AreEqual("25%", properties["padding-right"]);
        Assert.AreEqual("25%", properties["padding-bottom"]);
    }

    [TestMethod]
    public void CssPaddingSidesLengthLegal()
    {
        var properties = ParseProperties("padding: 10px 3em");

        Assert.AreEqual("3em", properties["padding-left"]);
        Assert.AreEqual("10px", properties["padding-top"]);
        Assert.AreEqual("3em", properties["padding-right"]);
        Assert.AreEqual("10px", properties["padding-bottom"]);
    }

    [TestMethod]
    public void CssPaddingThreeValuesLegal()
    {
        var properties = ParseProperties("padding: 10px 3em 5px");

        Assert.AreEqual("3em", properties["padding-left"]);
        Assert.AreEqual("10px", properties["padding-top"]);
        Assert.AreEqual("3em", properties["padding-right"]);
        Assert.AreEqual("5px", properties["padding-bottom"]);
    }

    [TestMethod]
    public void CssPaddingAllValuesWithPercentLegal()
    {
        var properties = ParseProperties("padding: 10px 5% 8px 2%");

        Assert.AreEqual("2%", properties["padding-left"]);
        Assert.AreEqual("10px", properties["padding-top"]);
        Assert.AreEqual("5%", properties["padding-right"]);
        Assert.AreEqual("8px", properties["padding-bottom"]);
    }

    [TestMethod]
    public void CssPaddingTooManyValuesIllegal()
    {
        var properties = ParseProperties("padding: 10px 5% 8px 2% 3px");

        Assert.IsFalse(properties.ContainsKey("padding-left"));
        Assert.IsFalse(properties.ContainsKey("padding-top"));
        Assert.IsFalse(properties.ContainsKey("padding-right"));
        Assert.IsFalse(properties.ContainsKey("padding-bottom"));
    }
}
