using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/PaddingProperty.cs.
/// Only the 1-4 value `padding` shorthand-splitting cases apply, mirroring MarginPropertyTests. The
/// individual longhand-property tests were dropped (out of scope per the porting brief).
/// The old <c>CssParser.ParseCssBlock</c> raw-property-dictionary API no longer exists - the CSS engine
/// port replaced it with a real, spec-compliant "padding" <c>ShorthandProperty</c> (the same vendored
/// engine PeachPDF itself uses), exercised here through <see cref="CssParser.ParseInlineStyle"/>.
/// Unlike the old hand-rolled <c>ParsePaddingProperty</c> (which only split on whitespace with no
/// keyword validation), the real engine's padding longhands only accept a length-percentage - "auto" is
/// correctly rejected (CssPaddingAutoIllegal, previously dropped as out of reach, is restored here).
/// </summary>
[TestClass]
public sealed class PaddingPropertyTests
{
    private static string GetProperty(string declaration, string propertyName)
    {
        var rule = new CssParser(new MockAdapter()).ParseInlineStyle(declaration);
        Assert.IsNotNull(rule);
        return rule.Style[propertyName];
    }

    [TestMethod]
    public void CssPaddingAllZeroLegal()
    {
        const string declaration = "padding: 0";

        Assert.AreEqual("0", GetProperty(declaration, "padding-left"));
        Assert.AreEqual("0", GetProperty(declaration, "padding-top"));
        Assert.AreEqual("0", GetProperty(declaration, "padding-right"));
        Assert.AreEqual("0", GetProperty(declaration, "padding-bottom"));
    }

    [TestMethod]
    public void CssPaddingAllPercentLegal()
    {
        const string declaration = "padding: 25%";

        Assert.AreEqual("25%", GetProperty(declaration, "padding-left"));
        Assert.AreEqual("25%", GetProperty(declaration, "padding-top"));
        Assert.AreEqual("25%", GetProperty(declaration, "padding-right"));
        Assert.AreEqual("25%", GetProperty(declaration, "padding-bottom"));
    }

    [TestMethod]
    public void CssPaddingSidesLengthLegal()
    {
        const string declaration = "padding: 10px 3em";

        Assert.AreEqual("3em", GetProperty(declaration, "padding-left"));
        Assert.AreEqual("10px", GetProperty(declaration, "padding-top"));
        Assert.AreEqual("3em", GetProperty(declaration, "padding-right"));
        Assert.AreEqual("10px", GetProperty(declaration, "padding-bottom"));
    }

    [TestMethod]
    public void CssPaddingAutoIllegal()
    {
        // "auto" is not a legal padding value (padding only accepts <length-percentage>) - restored
        // against the real engine, which now actually validates this (the old hand-rolled
        // ParsePaddingProperty had no keyword validation at all and let "auto" through unfiltered).
        const string declaration = "padding: auto";

        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "padding-left")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "padding-top")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "padding-right")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "padding-bottom")));
    }

    [TestMethod]
    public void CssPaddingThreeValuesLegal()
    {
        const string declaration = "padding: 10px 3em 5px";

        Assert.AreEqual("3em", GetProperty(declaration, "padding-left"));
        Assert.AreEqual("10px", GetProperty(declaration, "padding-top"));
        Assert.AreEqual("3em", GetProperty(declaration, "padding-right"));
        Assert.AreEqual("5px", GetProperty(declaration, "padding-bottom"));
    }

    [TestMethod]
    public void CssPaddingAllValuesWithPercentLegal()
    {
        const string declaration = "padding: 10px 5% 8px 2%";

        Assert.AreEqual("2%", GetProperty(declaration, "padding-left"));
        Assert.AreEqual("10px", GetProperty(declaration, "padding-top"));
        Assert.AreEqual("5%", GetProperty(declaration, "padding-right"));
        Assert.AreEqual("8px", GetProperty(declaration, "padding-bottom"));
    }

    [TestMethod]
    public void CssPaddingTooManyValuesIllegal()
    {
        const string declaration = "padding: 10px 5% 8px 2% 3px";

        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "padding-left")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "padding-top")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "padding-right")));
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty(declaration, "padding-bottom")));
    }
}
