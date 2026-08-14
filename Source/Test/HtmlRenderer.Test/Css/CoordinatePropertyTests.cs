using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/CoordinateProperty.cs.
/// Only the width/height/auto length-validation cases apply to HTML-Renderer: this fork's
/// CssBoxProperties has no Left/Right/Top/Bottom/MinWidth/MinHeight/MaxHeight CSS properties (only
/// Width, Height and MaxWidth exist - see CssBoxProperties.cs), so all left/top/right/bottom and
/// min-*/max-height source cases were dropped.
/// Validity is exercised through the real parsing pipeline (CssParser.ParseCssBlock -&gt;
/// CssParser.AddProperty -&gt; ParseLengthProperty -&gt; CssValueParser.IsValidLength), the same gate that
/// inline "style" attributes and stylesheet rules go through: an invalid width/height value is simply
/// dropped from the parsed property dictionary rather than being kept with some "has no value" flag.
/// </summary>
[TestClass]
public sealed class CoordinatePropertyTests
{
    private static IDictionary<string, string> ParseProperties(string declaration)
    {
        var parser = new CssParser(new MockAdapter());
        var block = parser.ParseCssBlock("test", declaration);
        Assert.IsNotNull(block);
        return block.Properties;
    }

    [TestMethod]
    public void CssHeightLegalPercentage()
    {
        var properties = ParseProperties("height: 28%");

        Assert.IsTrue(properties.ContainsKey("height"));
        Assert.AreEqual("28%", properties["height"]);
    }

    [TestMethod]
    public void CssHeightLegalLengthInEm()
    {
        var properties = ParseProperties("height: 0.3em");

        Assert.IsTrue(properties.ContainsKey("height"));
        Assert.AreEqual("0.3em", properties["height"]);
    }

    [TestMethod]
    public void CssHeightLegalLengthInPx()
    {
        var properties = ParseProperties("height: 144px");

        Assert.IsTrue(properties.ContainsKey("height"));
        Assert.AreEqual("144px", properties["height"]);
    }

    [TestMethod]
    public void CssHeightLegalAutoUppercase()
    {
        var properties = ParseProperties("height: AUTO");

        Assert.IsTrue(properties.ContainsKey("height"));
        Assert.AreEqual("auto", properties["height"]);
    }

    [TestMethod]
    public void CssWidthLegalLengthInCm()
    {
        var properties = ParseProperties("width: 0.5cm");

        Assert.IsTrue(properties.ContainsKey("width"));
        Assert.AreEqual("0.5cm", properties["width"]);
    }

    [TestMethod]
    public void CssWidthLegalLengthInMm()
    {
        var properties = ParseProperties("width: 1.5mm");

        Assert.IsTrue(properties.ContainsKey("width"));
        Assert.AreEqual("1.5mm", properties["width"]);
    }

    [TestMethod]
    public void CssWidthIllegalLength()
    {
        var properties = ParseProperties("width: 1.5 meter");

        Assert.IsFalse(properties.ContainsKey("width"));
    }

    [TestMethod]
    public void CssWidthPercentLegal()
    {
        var properties = ParseProperties("width: 20.5%");

        Assert.IsTrue(properties.ContainsKey("width"));
        Assert.AreEqual("20.5%", properties["width"]);
    }

    [TestMethod]
    public void CssWidthLegalLengthInInches()
    {
        var properties = ParseProperties("width: 3in");

        Assert.IsTrue(properties.ContainsKey("width"));
        Assert.AreEqual("3in", properties["width"]);
    }

    [TestMethod]
    public void CssHeightAngleIllegal()
    {
        var properties = ParseProperties("height: 3deg");

        Assert.IsFalse(properties.ContainsKey("height"));
    }

    [TestMethod]
    public void CssHeightResolutionIllegal()
    {
        var properties = ParseProperties("height: 3dpi");

        Assert.IsFalse(properties.ContainsKey("height"));
    }
}
