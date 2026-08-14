using System.Collections.Generic;
using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/WhiteSpaceProperty.cs.
/// PeachPDF.CSS models "white-space" as a typed, validating <c>WhiteSpaceProperty</c> (legal keywords are stored,
/// illegal ones are rejected and the property reports <c>HasValue == false</c>). HTML-Renderer has no such typed
/// property model: <see cref="TheArtOfDev.HtmlRenderer.Core.Dom.CssBoxProperties.WhiteSpace"/> is a plain string
/// set verbatim from whatever the CSS parser stored for the "white-space" declaration
/// (see <see cref="TheArtOfDev.HtmlRenderer.Core.Parse.CssParser"/>'s private AddProperty, which has no
/// dispatch case for "white-space" and so falls through to storing the (lower-cased) raw value unfiltered).
/// These tests therefore parse a small stylesheet through the real <see cref="CssData.Parse"/> /
/// <see cref="CssData.GetCssBlock"/> pipeline and read back the resulting "white-space" property text.
/// </summary>
[TestClass]
public sealed class WhiteSpacePropertyTests
{
    private static IDictionary<string, string> ParseDeclaration(string declaration)
    {
        var cssData = CssData.Parse(new MockAdapter(), $"test {{ {declaration} }}", false);
        return cssData.GetCssBlock("test").First().Properties;
    }

    [TestMethod]
    [DataRow("normal")]
    [DataRow("pre")]
    [DataRow("nowrap")]
    [DataRow("pre-wrap")]
    [DataRow("pre-line")]
    public void WhiteSpaceKeywordLegal(string keyword)
    {
        var properties = ParseDeclaration($"white-space: {keyword}");

        Assert.IsTrue(properties.ContainsKey("white-space"));
        Assert.AreEqual(keyword, properties["white-space"]);
    }

    [TestMethod]
    [Ignore("not yet spec compliant")]
    public void WhiteSpaceInvalidKeywordIllegal()
    {
        // PeachPDF: an invalid white-space keyword is rejected outright, so the property reports
        // HasValue == false (and, because "white-space" is flagged Inherited, IsInherited == true for the
        // failed parse). HTML-Renderer's CssParser performs no legality validation of "white-space" values at
        // all -- the raw (lower-cased) token is stored verbatim via the unconditional fallback branch in
        // CssParser.AddProperty. The intended/target behavior below (invalid value rejected, "white-space"
        // absent from the parsed properties) does not hold yet: currently "wavy" is stored as-is.
        var properties = ParseDeclaration("white-space: wavy");

        Assert.IsFalse(properties.ContainsKey("white-space"));
    }
}
