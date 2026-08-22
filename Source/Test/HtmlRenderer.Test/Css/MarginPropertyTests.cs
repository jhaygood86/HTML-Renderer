using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/MarginProperty.cs.
/// Only the 1-4 value `margin` shorthand-splitting cases apply. The individual longhand-property
/// tests (margin-left, margin-right, margin-top, margin-bottom) and the CSS text
/// recombination/simplification tests (MarginShouldBeRecombinedCorrectly and friends, which depend on
/// PeachPDF's own CSSOM re-serializing a rule back to text) have no equivalent surface in HTML-Renderer
/// and were dropped.
/// The old <c>CssParser.ParseCssBlock</c> raw-property-dictionary API no longer exists - the CSS engine
/// port replaced it with a real, spec-compliant "margin" <c>ShorthandProperty</c> (the same vendored
/// engine PeachPDF itself uses), exercised here through <see cref="CssParser.ParseInlineStyle"/> and its
/// resulting <c>StyleDeclaration</c>'s typed margin-* longhands. Per CSS Box Model: 1 value applies to
/// all sides, 2 values are (vertical, horizontal), 3 values are (top, horizontal, bottom), 4 values are
/// (top, right, bottom, left); a value with any other token count is invalid and leaves the shorthand -
/// and so every margin-* longhand - entirely unset.
/// </summary>
[TestClass]
public sealed class MarginPropertyTests
{
    private static string GetProperty(string declaration, string propertyName)
    {
        var rule = new CssParser(new MockAdapter()).ParseInlineStyle(declaration);
        Assert.IsNotNull(rule);
        return rule.Style[propertyName];
    }

    [TestMethod]
    public void MarginAllZeroLegal()
    {
        const string declaration = "margin: 0";

        Assert.AreEqual("0", GetProperty(declaration, "margin-left"));
        Assert.AreEqual("0", GetProperty(declaration, "margin-top"));
        Assert.AreEqual("0", GetProperty(declaration, "margin-right"));
        Assert.AreEqual("0", GetProperty(declaration, "margin-bottom"));
    }

    [TestMethod]
    public void MarginAllPercentLegal()
    {
        const string declaration = "margin: 25%";

        Assert.AreEqual("25%", GetProperty(declaration, "margin-left"));
        Assert.AreEqual("25%", GetProperty(declaration, "margin-top"));
        Assert.AreEqual("25%", GetProperty(declaration, "margin-right"));
        Assert.AreEqual("25%", GetProperty(declaration, "margin-bottom"));
    }

    [TestMethod]
    public void MarginAutoLegal()
    {
        const string declaration = "margin: auto";

        Assert.AreEqual("auto", GetProperty(declaration, "margin-left"));
        Assert.AreEqual("auto", GetProperty(declaration, "margin-top"));
        Assert.AreEqual("auto", GetProperty(declaration, "margin-right"));
        Assert.AreEqual("auto", GetProperty(declaration, "margin-bottom"));
    }

    [TestMethod]
    public void MarginSidesLengthLegal()
    {
        const string declaration = "margin: 10px 3em";

        Assert.AreEqual("3em", GetProperty(declaration, "margin-left"));
        Assert.AreEqual("10px", GetProperty(declaration, "margin-top"));
        Assert.AreEqual("3em", GetProperty(declaration, "margin-right"));
        Assert.AreEqual("10px", GetProperty(declaration, "margin-bottom"));
    }

    [TestMethod]
    public void MarginSidesLengthAndAutoLegal()
    {
        const string declaration = "margin: 10px auto";

        Assert.AreEqual("auto", GetProperty(declaration, "margin-left"));
        Assert.AreEqual("10px", GetProperty(declaration, "margin-top"));
        Assert.AreEqual("auto", GetProperty(declaration, "margin-right"));
        Assert.AreEqual("10px", GetProperty(declaration, "margin-bottom"));
    }

    [TestMethod]
    public void MarginThreeValuesLegal()
    {
        const string declaration = "margin: 10px 3em 5px";

        Assert.AreEqual("3em", GetProperty(declaration, "margin-left"));
        Assert.AreEqual("10px", GetProperty(declaration, "margin-top"));
        Assert.AreEqual("3em", GetProperty(declaration, "margin-right"));
        Assert.AreEqual("5px", GetProperty(declaration, "margin-bottom"));
    }

    [TestMethod]
    public void MarginAllValuesWithPercentAndAutoLegal()
    {
        const string declaration = "margin: 10px 5% auto 2%";

        Assert.AreEqual("2%", GetProperty(declaration, "margin-left"));
        Assert.AreEqual("10px", GetProperty(declaration, "margin-top"));
        Assert.AreEqual("5%", GetProperty(declaration, "margin-right"));
        Assert.AreEqual("auto", GetProperty(declaration, "margin-bottom"));
    }

    [TestMethod]
    public void MarginTooManyValuesIllegal()
    {
        const string declaration = "margin: 10px 5% 8px 2% 3px auto";

        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "margin-left")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "margin-top")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "margin-right")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "margin-bottom")));
    }
}
