using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>Ported from PeachPDF.Tests/CSS/FontFace.cs.</summary>
[TestClass]
public sealed class FontFaceTests
{
    [TestMethod]
    public void FontFaceOpenSansWithSource()
    {
        var src = "@font-face{font-family:'Open Sans';src:url(fonts/OpenSans-Light.eot);src:local('Open Sans Light'),local('OpenSans-Light'),url(fonts/OpenSans-Light.ttf) format('truetype'),url(fonts/OpenSans-Light.woff) format('woff');font-style:normal}";
        var sheet = CssConstructionFunctions.ParseStyleSheet(src);
        Assert.IsNotNull(sheet);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<FontFaceRule>(sheet.Rules[0]);
        var fontface = (IFontFaceRule)sheet.Rules[0];
        Assert.AreEqual("\"Open Sans\"", fontface.Family);
        Assert.AreEqual("", fontface.Features);
        Assert.AreEqual("", fontface.Range);
        Assert.AreNotEqual("", fontface.Source);
        Assert.AreEqual("", fontface.Stretch);
        Assert.AreEqual("normal", fontface.Style);
        Assert.AreEqual("", fontface.Variant);
        Assert.AreEqual("", fontface.Weight);
    }

    [TestMethod]
    public void FontFaceWithWoff2Source()
    {
        var src = "@font-face{font-family:'Inter';src:url(fonts/Inter-Medium.woff2) format('woff2');font-weight:500}";
        var sheet = CssConstructionFunctions.ParseStyleSheet(src);
        Assert.IsNotNull(sheet);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<FontFaceRule>(sheet.Rules[0]);
        var fontface = (IFontFaceRule)sheet.Rules[0];
        Assert.AreEqual("\"Inter\"", fontface.Family);
        StringAssert.Contains(fontface.Source, "Inter-Medium.woff2");
        StringAssert.Contains(fontface.Source, "woff2");
        Assert.AreEqual("500", fontface.Weight);
    }

    [TestMethod]
    public void FontFaceOpenSansNoSource()
    {
        var src = "@font-face{font-family:'Open Sans';font-style:normal}";
        var sheet = CssConstructionFunctions.ParseStyleSheet(src);
        Assert.IsNotNull(sheet);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<FontFaceRule>(sheet.Rules[0]);
        var fontface = (IFontFaceRule)sheet.Rules[0];
        Assert.AreEqual("\"Open Sans\"", fontface.Family);
        Assert.AreEqual("", fontface.Features);
        Assert.AreEqual("", fontface.Range);
        Assert.AreEqual("", fontface.Source);
        Assert.AreEqual("", fontface.Stretch);
        Assert.AreEqual("normal", fontface.Style);
        Assert.AreEqual("", fontface.Variant);
        Assert.AreEqual("", fontface.Weight);
    }

    [TestMethod]
    [DataRow("U+41-5A")]
    [DataRow("U+0-7F")]
    [DataRow("U+4??")]
    [DataRow("U+000041")]
    [DataRow("U+0025-00FF, U+4??")]
    public void FontFaceUnicodeRangeIsRetained(string range)
    {
        // Data holds the range without its "U+" prefix, so a retained descriptor used to serialize as
        // "41-5A" - not a valid <urange> and not what was authored.
        var src = "@font-face{font-family:'Open Sans';unicode-range:" + range + "}";
        var sheet = CssConstructionFunctions.ParseStyleSheet(src);
        Assert.AreEqual(1, sheet.Rules.Length);
        var fontface = (IFontFaceRule)sheet.Rules[0];

        Assert.AreEqual(range, fontface.Range);
    }

    [TestMethod]
    // The wildcard budget is captured up front; re-evaluating "6 - StringBuffer.Length" while appending
    // to that same buffer consumed only half the available wildcards, so U+?????? stopped after three.
    [DataRow("U+4??", "400", "4FF")]
    [DataRow("U+??????", "000000", "FFFFFF")]
    [DataRow("U+41", "41", "41")]
    [DataRow("U+41-5A", "41", "5A")]
    [DataRow("U+0-7F", "0", "7F")]
    public void UnicodeRangeTokenStartAndEnd(string source, string start, string end)
    {
        var lexer = new Lexer(new TextSource(source));
        var range = lexer.Get();
        Assert.IsInstanceOfType<RangeToken>(range);
        var rangeToken = (RangeToken)range;

        Assert.AreEqual(start, rangeToken.Start);
        Assert.AreEqual(end, rangeToken.End);
        Assert.AreEqual(TokenType.EndOfFile, lexer.Get().Type);
    }
}
