using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/CoordinateProperty.cs.
/// Only the width/height/auto length-validation cases apply to HTML-Renderer: this fork's
/// CssBoxProperties has no Left/Right/Top/Bottom/MinWidth/MinHeight/MaxHeight CSS properties (only
/// Width, Height and MaxWidth exist - see CssBoxProperties.cs), so all left/top/right/bottom and
/// min-*/max-height source cases were dropped.
/// The old <c>CssParser.ParseCssBlock</c>/<c>CssData.GetCssBlock</c> raw-property-dictionary API this
/// file originally used no longer exists - the CSS engine port replaced it with a real, spec-compliant
/// parser (ported from ExCSS via PeachPDF) whose only declaration-level introspection surface is
/// <see cref="CssParser.ParseInlineStyle"/>, returning an <c>IStyleRule</c> whose <c>Style</c>
/// (a <c>StyleDeclaration</c>) exposes each longhand as a typed, validating property - an invalid value
/// is simply never stored, so <c>GetPropertyValue</c>/the string indexer returns "" rather than throwing
/// or keeping a "has no value" flag. Validity is exercised through that same real pipeline (tokenizer -&gt;
/// grammar -&gt; value converter) that inline "style" attributes and stylesheet rules go through.
/// </summary>
[TestClass]
public sealed class CoordinatePropertyTests
{
    private static string GetProperty(string declaration, string propertyName)
    {
        var rule = new CssParser(new MockAdapter()).ParseInlineStyle(declaration);
        Assert.IsNotNull(rule);
        return rule.Style[propertyName];
    }

    [TestMethod]
    public void CssHeightLegalPercentage()
    {
        Assert.AreEqual("28%", GetProperty("height: 28%", "height"));
    }

    [TestMethod]
    public void CssHeightLegalLengthInEm()
    {
        Assert.AreEqual("0.3em", GetProperty("height: 0.3em", "height"));
    }

    [TestMethod]
    public void CssHeightLegalLengthInPx()
    {
        Assert.AreEqual("144px", GetProperty("height: 144px", "height"));
    }

    [TestMethod]
    public void CssHeightLegalAutoUppercase()
    {
        Assert.AreEqual("auto", GetProperty("height: AUTO", "height"));
    }

    [TestMethod]
    public void CssWidthLegalLengthInCm()
    {
        Assert.AreEqual("0.5cm", GetProperty("width: 0.5cm", "width"));
    }

    [TestMethod]
    public void CssWidthLegalLengthInMm()
    {
        Assert.AreEqual("1.5mm", GetProperty("width: 1.5mm", "width"));
    }

    [TestMethod]
    public void CssWidthIllegalLength()
    {
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty("width: 1.5 meter", "width")));
    }

    [TestMethod]
    public void CssWidthPercentLegal()
    {
        Assert.AreEqual("20.5%", GetProperty("width: 20.5%", "width"));
    }

    [TestMethod]
    public void CssWidthLegalLengthInInches()
    {
        Assert.AreEqual("3in", GetProperty("width: 3in", "width"));
    }

    [TestMethod]
    public void CssHeightAngleIllegal()
    {
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty("height: 3deg", "height")));
    }

    [TestMethod]
    public void CssHeightResolutionIllegal()
    {
        Assert.IsTrue(string.IsNullOrEmpty(GetProperty("height: 3dpi", "height")));
    }
}
