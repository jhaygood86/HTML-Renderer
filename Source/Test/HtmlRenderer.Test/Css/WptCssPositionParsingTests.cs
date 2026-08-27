using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/WptCssPositionParsingTests.cs.
/// Grammar coverage for <c>position</c>, the four physical inset longhands (<c>top</c>/<c>right</c>/
/// <c>bottom</c>/<c>left</c>), and <c>z-index</c>, adapted from the web-platform-tests
/// css/css-position/parsing/ fixtures (position-valid.html, position-invalid.html, top/right/bottom/
/// left-valid.html, top/right/bottom/left-invalid.html, z-index-valid.html, z-index-invalid.html).
/// WPT's test_valid_value/test_invalid_value assert against a live getComputedStyle(); this engine has
/// no CSSOM computed-style API to match that against, so the equivalent here is whether the declaration
/// round-trips through <see cref="StyleDeclaration.GetPropertyValue"/> (accepted) or is dropped
/// entirely, leaving an empty string (rejected).
/// <c>position</c> (<see cref="PositionProperty"/>), <c>top</c>/<c>right</c>/<c>bottom</c>/<c>left</c>
/// (Coordinate/*.cs), and non-calc() <c>z-index</c> (<see cref="ZIndexProperty"/>) all exist with matching
/// accept/reject behavior.
/// </summary>
[TestClass]
public sealed class WptCssPositionParsingTests
{
    private static string DeclaredValue(string property, string value)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ {property}: {value}; }}");
        var rule = sheet.Rules.OfType<StyleRule>().Single();
        return rule.Style.GetPropertyValue(property);
    }

    // ─── position: 'static | relative | absolute | sticky | fixed' ───────────

    [TestMethod]
    [DataRow("static")]
    [DataRow("relative")]
    [DataRow("absolute")]
    [DataRow("sticky")]
    [DataRow("fixed")]
    public void Position_AcceptsValidKeyword(string value)
    {
        Assert.AreEqual(value, DeclaredValue("position", value));
    }

    [TestMethod]
    [DataRow("auto")]
    [DataRow("static relative")]
    public void Position_RejectsInvalidValue(string value)
    {
        Assert.AreEqual(string.Empty, DeclaredValue("position", value));
    }

    // ─── top/right/bottom/left: 'auto | <length-percentage>' ─────────────────

    [TestMethod]
    [DataRow("top")]
    [DataRow("right")]
    [DataRow("bottom")]
    [DataRow("left")]
    public void Coordinate_AcceptsAuto(string property)
    {
        Assert.AreEqual("auto", DeclaredValue(property, "auto"));
    }

    [TestMethod]
    [DataRow("top")]
    [DataRow("right")]
    [DataRow("bottom")]
    [DataRow("left")]
    public void Coordinate_AcceptsNegativeLength(string property)
    {
        Assert.AreEqual("-10px", DeclaredValue(property, "-10px"));
    }

    [TestMethod]
    [DataRow("top")]
    [DataRow("right")]
    [DataRow("bottom")]
    [DataRow("left")]
    public void Coordinate_AcceptsNegativePercentage(string property)
    {
        Assert.AreEqual("-20%", DeclaredValue(property, "-20%"));
    }

    [TestMethod]
    [DataRow("top")]
    [DataRow("right")]
    [DataRow("bottom")]
    [DataRow("left")]
    public void Coordinate_AcceptsCalcExpression(string property)
    {
        Assert.IsFalse(string.IsNullOrEmpty(DeclaredValue(property, "calc(2em + 3ex)")));
    }

    [TestMethod]
    [DataRow("top")]
    [DataRow("right")]
    [DataRow("bottom")]
    [DataRow("left")]
    public void Coordinate_RejectsNonLengthKeyword(string property)
    {
        Assert.AreEqual(string.Empty, DeclaredValue(property, "min-content"));
    }

    [TestMethod]
    [DataRow("top")]
    [DataRow("right")]
    [DataRow("bottom")]
    [DataRow("left")]
    public void Coordinate_RejectsUnitlessNumber(string property)
    {
        Assert.AreEqual(string.Empty, DeclaredValue(property, "60"));
    }

    [TestMethod]
    [DataRow("top")]
    [DataRow("right")]
    [DataRow("bottom")]
    [DataRow("left")]
    public void Coordinate_RejectsMultipleValues(string property)
    {
        Assert.AreEqual(string.Empty, DeclaredValue(property, "10px 20%"));
    }

    // ─── z-index: 'auto | <integer>' ──────────────────────────────────────────

    [TestMethod]
    [DataRow("auto")]
    [DataRow("-789")]
    [DataRow("0")]
    [DataRow("123")]
    public void ZIndex_AcceptsValidValue(string value)
    {
        Assert.AreEqual(value, DeclaredValue("z-index", value));
    }

    [TestMethod]
    [DataRow("none")]
    [DataRow("10px")]
    [DataRow("0.5")]
    [DataRow("auto 123")]
    public void ZIndex_RejectsInvalidValue(string value)
    {
        Assert.AreEqual(string.Empty, DeclaredValue("z-index", value));
    }
}
