using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/RealWorld.cs.
/// PeachPDF's version parses a stylesheet with its <c>PeachPDF.CSS</c> engine (a fork of ExCSS)'s <c>StylesheetParser</c> and reads the
/// parsed rule's properties back as CSSOM-normalized strings (e.g. "rgb(90, 94, 237)"). HTML-Renderer
/// has no CSSOM: <see cref="CssParser"/> parses a stylesheet straight into <see cref="TheArtOfDev.HtmlRenderer.Core.CssData"/>
/// buckets of raw (lower-cased) property strings, so colors are read back as parsed (e.g. "#5a5eed"),
/// with <see cref="CssParser.ParseColor"/> used to also verify the resolved RGB value.
/// The second PeachPDF test (CreateStylesheet_WithCssProperties_ExpectStandardStringBack) round-trips
/// a rule back through <c>ToCss()</c> - there is no CSS serialization (CSSOM) in HTML-Renderer, so that
/// test has no equivalent here and was dropped entirely.
/// </summary>
[TestClass]
public sealed class RealWorldTests
{
    [TestMethod]
    public void ParseCss_WithStandardString_ExpectReadableProperties()
    {
        // Could read in a file here...
        // Arrange
        const string css = "html{ background-color: #5a5eed; color: #FFFFFF; margin: 5px; } h2{ background-color: red }";

        var parser = new CssParser(new MockAdapter());

        // Act
        var cssData = parser.ParseStyleSheet(css, false);

        var htmlBlock = cssData.GetCssBlock("html").First();
        var h2Block = cssData.GetCssBlock("h2").First();

        var backgroundColor = htmlBlock.Properties["background-color"];
        var foregroundColor = htmlBlock.Properties["color"];

        // Assert
        Assert.AreEqual("html", htmlBlock.Class);
        Assert.AreEqual("#5a5eed", backgroundColor);
        Assert.AreEqual(RColor.FromArgb(90, 94, 237), parser.ParseColor(backgroundColor));
        Assert.AreEqual("#ffffff", foregroundColor);
        Assert.AreEqual(RColor.FromArgb(255, 255, 255), parser.ParseColor(foregroundColor));

        // "margin" has no single longhand equivalent in HTML-Renderer - the shorthand is split into
        // margin-left/top/right/bottom at parse time (CssParser.ParseMarginProperty).
        Assert.AreEqual("5px", htmlBlock.Properties["margin-left"]);
        Assert.AreEqual("5px", htmlBlock.Properties["margin-top"]);
        Assert.AreEqual("5px", htmlBlock.Properties["margin-right"]);
        Assert.AreEqual("5px", htmlBlock.Properties["margin-bottom"]);

        Assert.AreEqual("h2", h2Block.Class);
        Assert.AreEqual(RColor.FromArgb(255, 0, 0), parser.ParseColor(h2Block.Properties["background-color"]));
    }
}
