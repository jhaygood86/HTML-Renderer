using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/Property.cs (the Color/Content/Quotes cases).
///
/// color (StyleProperties/Font/ColorProperty.cs) uses <c>Converters.ColorConverter</c>, which resolves
/// hex/named colors to a normalized <c>rgb(r, g, b)</c>/<c>rgba(r, g, b, a)</c> text form while keeping
/// <c>Original</c> as authored - confirmed identical to the color normalization already exercised by
/// BorderPropertyTests.cs's border-color/border-*-color ports.
///
/// content (StyleProperties/ContentProperty.cs) accepts <c>normal</c>, <c>none</c>, one or more
/// &lt;string&gt;s, <c>url()</c>, and the open-quote/close-quote/no-open-quote/no-close-quote keywords
/// (all present in Enumerations/Keywords.cs) via <c>ContentModes.ToConverter().Or(UrlConverter)...Many()</c>.
///
/// quotes (StyleProperties/QuotesProperty.cs) uses <c>Converters.EvenStringsConverter.OrNone()</c> - an
/// even (2, 4, ...) list of quote-mark string pairs, or the literal keyword <c>none</c>; an odd count or
/// non-string tokens are rejected.
/// </summary>
[TestClass]
public sealed class PropertyContentTests
{
    [TestMethod]
    public void CssColorHexLegal()
    {
        var snippet = "color : #123456";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColorProperty>(property);
        var concrete = (ColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(18, 52, 86)", concrete.Value);
        Assert.AreEqual("#123456", concrete.Original);
    }

    [TestMethod]
    public void CssColorRgbLegal()
    {
        var snippet = "color : rgb(121, 181, 201)";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColorProperty>(property);
        var concrete = (ColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(121, 181, 201)", concrete.Value);
        Assert.AreEqual("rgb(121, 181, 201)", concrete.Original);
    }

    [TestMethod]
    public void CssColorRgbaLegal()
    {
        var snippet = "color : rgba(255, 255, 201, 0.7)";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColorProperty>(property);
        var concrete = (ColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgba(255, 255, 201, 0.7)", concrete.Value);
        Assert.AreEqual("rgba(255, 255, 201, 0.7)", concrete.Original);
    }

    [TestMethod]
    public void CssColorNameLegal()
    {
        var snippet = "color : red";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColorProperty>(property);
        var concrete = (ColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(255, 0, 0)", concrete.Value);
        Assert.AreEqual("red", concrete.Original);
    }

    [TestMethod]
    public void CssColorNameUppercaseLegal()
    {
        var snippet = "color : BLUE";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColorProperty>(property);
        var concrete = (ColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(0, 0, 255)", concrete.Value);
        Assert.AreEqual("BLUE", concrete.Original);
    }

    [TestMethod]
    public void CssColorNameIllegal()
    {
        var snippet = "color : horse";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColorProperty>(property);
        var concrete = (ColorProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssContentNormalLegal()
    {
        var snippet = "content : normal ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("content", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ContentProperty>(property);
        var concrete = (ContentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("normal", concrete.Value);
        Assert.AreEqual("normal", concrete.Original);
    }

    [TestMethod]
    public void CssContentNoneLegalUppercaseN()
    {
        var snippet = "content : noNe ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("content", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ContentProperty>(property);
        var concrete = (ContentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
        Assert.AreEqual("noNe", concrete.Original);
    }

    [TestMethod]
    public void CssContentStringLegal()
    {
        var snippet = "content : 'hi' ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("content", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ContentProperty>(property);
        var concrete = (ContentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("\"hi\"", concrete.Value);
        Assert.AreEqual("\"hi\"", concrete.Original);
    }

    [TestMethod]
    public void CssContentNoOpenQuoteNoCloseQuoteLegal()
    {
        var snippet = "content : no-open-quote no-close-quote ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("content", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ContentProperty>(property);
        var concrete = (ContentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("no-open-quote no-close-quote", concrete.Value);
        Assert.AreEqual("no-open-quote no-close-quote", concrete.Original);
    }

    [TestMethod]
    public void CssContentUrlLegal()
    {
        var snippet = "content : url(test.html) ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("content", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ContentProperty>(property);
        var concrete = (ContentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("url(\"test.html\")", concrete.Value);
        Assert.AreEqual("url(\"test.html\")", concrete.Original);
    }

    [TestMethod]
    public void CssContentStringsLegal()
    {
        var snippet = "content : 'how' 'are' 'you' ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("content", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ContentProperty>(property);
        var concrete = (ContentProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("\"how\" \"are\" \"you\"", concrete.Value);
        Assert.AreEqual("\"how\" \"are\" \"you\"", concrete.Original);
    }

    [TestMethod]
    public void CssQuoteStringIllegal()
    {
        var snippet = "quotes : '\"' ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("quotes", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<QuotesProperty>(property);
        var concrete = (QuotesProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssQuoteStringsLegal()
    {
        var snippet = "quotes : '\"' '\"' ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("quotes", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<QuotesProperty>(property);
        var concrete = (QuotesProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("\"\\\"\" \"\\\"\"", concrete.Value);
        Assert.AreEqual("\"\\\"\" \"\\\"\"", concrete.Original);
    }

    [TestMethod]
    public void CssQuoteStringsIllegal()
    {
        var snippet = "quotes : \"'\"";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("quotes", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<QuotesProperty>(property);
        var concrete = (QuotesProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssQuoteStringsMultipleLegal()
    {
        // The fourth quote mark is a literal U+FFFD REPLACEMENT CHARACTER, exactly as authored in
        // PeachPDF's own source file (src/PeachPDF.Tests/CSS/Property.cs) - not a mangled encoding here.
        var snippet = "quotes : '\"' '\"' '`' '�' ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("quotes", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<QuotesProperty>(property);
        var concrete = (QuotesProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("\"\\\"\" \"\\\"\" \"`\" \"�\"", concrete.Value);
        Assert.AreEqual("\"\\\"\" \"\\\"\" \"`\" \"�\"", concrete.Original);
    }

    [TestMethod]
    public void CssQuoteStringsMultipleIllegal()
    {
        var snippet = "quotes : '\"' '\"' '`' ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("quotes", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<QuotesProperty>(property);
        var concrete = (QuotesProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssQuoteNoneLegal()
    {
        var snippet = "quotes : none";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("quotes", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<QuotesProperty>(property);
        var concrete = (QuotesProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
        Assert.AreEqual("none", concrete.Original);
    }

    [TestMethod]
    public void CssQuoteNoneStringIllegal()
    {
        var snippet = "quotes : 'none'";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("quotes", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<QuotesProperty>(property);
        var concrete = (QuotesProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssQuoteNormalIllegal()
    {
        var snippet = "quotes : normal ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("quotes", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<QuotesProperty>(property);
        var concrete = (QuotesProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }
}
