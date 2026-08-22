using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/FontPaletteParsingTests.cs.
/// Tests for <see cref="FontPaletteValueConverter"/> (the <c>font-palette</c> property) and
/// <see cref="FontPaletteValuesRule"/> (the <c>@font-palette-values</c> at-rule), confirmed an exact
/// match against PeachPDF's implementation.
/// </summary>
[TestClass]
public sealed class FontPaletteParsingTests
{
    private static string FontPalette(string value)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ font-palette: {value}; }}");
        var rule = sheet.Rules.OfType<StyleRule>().Single();
        return rule.Style.GetPropertyValue("font-palette");
    }

    [TestMethod]
    [DataRow("normal")]
    [DataRow("light")]
    [DataRow("dark")]
    [DataRow("--my-palette")]
    [DataRow("palette-mix(in oklab, --a, --b)")]
    [DataRow("palette-mix(in lch longer hue, normal 30%, --b 70%)")]
    public void FontPalette_AcceptsValidValues(string value)
    {
        Assert.AreEqual(value, FontPalette(value));
    }

    [TestMethod]
    [DataRow("5px")]
    [DataRow("#fff")]
    [DataRow("rgb(0,0,0)")]
    [DataRow("palette-mix(in oklab, --a)")]              // needs two operands
    [DataRow("palette-mix(in bogusspace, --a, --b)")]   // unknown color space
    [DataRow("palette-mix(--a, --b)")]                  // missing color-interpolation-method
    public void FontPalette_DropsInvalidValues(string value)
    {
        Assert.AreEqual(string.Empty, FontPalette(value));
    }

    [TestMethod]
    public void FontPaletteValues_ParsesNameAndDescriptors()
    {
        var src = "@font-palette-values --brand { font-family: \"Nabla\"; base-palette: 2; override-colors: 0 #ff0000, 1 rgb(0, 255, 0); }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(src);

        var rule = sheet.Rules.OfType<FontPaletteValuesRule>().Single();
        Assert.IsInstanceOfType<FontPaletteValuesRule>(rule);
        Assert.AreEqual("--brand", rule.Name);
        StringAssert.Contains(rule.Family, "Nabla");
        Assert.AreEqual("2", rule.BasePalette);
        StringAssert.Contains(rule.OverrideColors, "#ff0000");
        StringAssert.Contains(rule.OverrideColors, "rgb(0, 255, 0)");
    }

    [TestMethod]
    public void FontPaletteValues_LightDarkBasePalette()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@font-palette-values --d { font-family: Nabla; base-palette: dark; }");
        var rule = sheet.Rules.OfType<FontPaletteValuesRule>().Single();
        Assert.AreEqual("dark", rule.BasePalette);
    }

    [TestMethod]
    public void FontPaletteValues_DoesNotDerailFollowingRules()
    {
        var src = "@font-palette-values --p { font-family: Nabla; base-palette: 1; } .after { color: red; }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(src);

        Assert.AreEqual(1, sheet.Rules.OfType<FontPaletteValuesRule>().Count());
        var styleRule = sheet.Rules.OfType<StyleRule>().Single();
        Assert.AreEqual(".after", styleRule.SelectorText);
        Assert.AreEqual("rgb(255, 0, 0)", styleRule.Style.GetPropertyValue("color"));
    }

    [TestMethod]
    public void FontPaletteValues_NoDeclarationBlock_DoesNotCrashAndFollowingRuleApplies()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@font-palette-values --x; .after { color: red; }");
        var styleRule = sheet.Rules.OfType<StyleRule>().Single();
        Assert.AreEqual(".after", styleRule.SelectorText);
    }
}
