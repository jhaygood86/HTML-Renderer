using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/CustomPropertyTests.cs. Verifies that the CSS module
/// correctly parses custom properties (--foo) and tolerates the var() function inside any property's
/// value. Pure CSSOM parse tests - no layout/cascade resolution of var() is exercised here.
/// </summary>
[TestClass]
public sealed class CustomPropertyTests
{
    [TestMethod]
    public void CustomProperty_SimpleValue_ParsesAndRoundTrips()
    {
        var property = CssConstructionFunctions.ParseDeclaration("--main-color: red");
        Assert.AreEqual("--main-color", property.Name);
        Assert.IsInstanceOfType<CustomProperty>(property);
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("red", property.Value);
    }

    [TestMethod]
    public void CustomProperty_ArbitraryTokenSoup_NeverFailsToParse()
    {
        var property = CssConstructionFunctions.ParseDeclaration("--x: 1px solid   red , foo(bar)");
        Assert.AreEqual("--x", property.Name);
        Assert.IsInstanceOfType<CustomProperty>(property);
        Assert.IsTrue(property.HasValue);
    }

    [TestMethod]
    [DataRow("--x: #hero", "#hero")] // an id-shaped hash-token value (CSS Syntax accepts it)
    [DataRow("--x: #f00", "#f00")] // a hex color hash
    public void CustomProperty_HashValue_ParsesAndRoundTrips(string snippet, string expected)
    {
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("--x", property.Name);
        Assert.IsInstanceOfType<CustomProperty>(property);
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual(expected, property.Value);
    }

    [TestMethod]
    public void CustomProperty_WithVarReference_RoundTripsLiterally()
    {
        var property = CssConstructionFunctions.ParseDeclaration("--b: var(--a, blue)");
        Assert.AreEqual("--b", property.Name);
        Assert.IsTrue(property.HasValue);
        StringAssert.Contains(property.Value, "var(--a, blue)");
    }

    [TestMethod]
    public void CustomProperty_NameIsCaseSensitive()
    {
        var lower = CssConstructionFunctions.ParseDeclaration("--foo: 1px");
        var upper = CssConstructionFunctions.ParseDeclaration("--Foo: 2px");
        Assert.AreEqual("--foo", lower.Name);
        Assert.AreEqual("--Foo", upper.Name);
    }

    [TestMethod]
    public void CustomProperty_MultiHyphenatedName_ParsesAsSingleIdentifier()
    {
        var property = CssConstructionFunctions.ParseDeclaration("--foo-bar-baz: 1px");
        Assert.AreEqual("--foo-bar-baz", property.Name);
    }

    [TestMethod]
    public void ColorProperty_WithVarFunction_DoesNotFailToParse()
    {
        var property = CssConstructionFunctions.ParseDeclaration("color: var(--main-color, red)");
        Assert.AreEqual("color", property.Name);
        Assert.IsTrue(property.HasValue);
        StringAssert.Contains(property.Value, "var(--main-color, red)");
    }

    [TestMethod]
    public void MarginProperty_WithMultipleVarFunctions_DoesNotFailToParse()
    {
        var property = CssConstructionFunctions.ParseDeclaration("margin: var(--a) var(--b)");
        Assert.AreEqual("margin", property.Name);
        Assert.IsTrue(property.HasValue);
    }

    [TestMethod]
    public void MarginShorthandInStyleSheet_WithVar_KeepsShorthandWholeUntilCascadeTime()
    {
        // Shorthand-to-longhand expansion happens at parse time (StyleDeclaration.SetShorthand);
        // a var() reference can't be split into per-longhand slices until it's resolved per-element,
        // so the shorthand must survive intact for CssUtils.SetPropertyValue to expand post-substitution.
        var sheet = CssConstructionFunctions.ParseStyleSheet("* { margin: var(--a) var(--b); }");
        var rule = sheet.StyleRules.Single();
        var names = rule.Style.Select(p => p.Name).ToArray();
        CollectionAssert.AreEqual(new[] { "margin" }, names);
    }

    [TestMethod]
    public void Property_WithNestedVarInsideOtherFunction_DoesNotFailToParse()
    {
        var property = CssConstructionFunctions.ParseDeclaration("background-image: linear-gradient(var(--c1), var(--c2))");
        Assert.AreEqual("background-image", property.Name);
        Assert.IsTrue(property.HasValue);
    }

    // ── Lexer regression coverage for the -- identifier-start fix ──────────

    [TestMethod]
    public void HtmlCommentClose_StillTokenizesAsCdc_NotAffectedByDashFix()
    {
        var stylesheet = CssConstructionFunctions.ParseStyleSheet("<!-- .a { color: red; } -->");
        Assert.IsTrue(stylesheet.StyleRules.Any());
    }

    [TestMethod]
    public void VendorPrefixedProperty_StillParsesAsUnknownSingleHyphenIdent()
    {
        var property = CssConstructionFunctions.ParseDeclaration("-webkit-transform: none", includeUnknownDeclarations: true);
        Assert.AreEqual("-webkit-transform", property.Name);
    }

    [TestMethod]
    public void NegativeLength_StillParsesCorrectly()
    {
        var property = CssConstructionFunctions.ParseDeclaration("margin-top: -5px");
        Assert.AreEqual("margin-top", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("-5px", property.Value);
    }
}
