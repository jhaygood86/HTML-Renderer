using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/FontProperty.cs (source class <c>CssFontPropertyTests</c>), the <c>font</c>
/// shorthand cases. Exercises <see cref="FontProperty"/>
/// (Source/HtmlRenderer/Core/CssEngine/StyleProperties/Font/FontProperty.cs), whose converter is
/// <c>WithOrder(WithAny(style, variant, weight, stretch), WithOrder(size.Required(),
/// lineHeight.StartsWithDelimiter().Option(), family.Required())).Or(SystemFontConverter).OrGlobalValue()</c>
/// (Model/Converters.cs) - so any subset of style/variant/weight/stretch may appear in any order before a
/// required size (optionally "/" line-height) and a required family list, or the whole value may instead be
/// one of the <c>Map.SystemFonts</c> keywords (caption/icon/menu/message-box/small-caption/status-bar) or a
/// CSS-wide keyword. <c>PropertyFactory.CreateFont</c>
/// (Source/HtmlRenderer/Core/CssEngine/Factories/PropertyFactory.cs) registers all seven longhands
/// (font-family, font-size, font-stretch, font-style, font-variant, font-weight, line-height) under this
/// shorthand, matching PeachPDF's own registration.
/// </summary>
[TestClass]
public sealed class FontShorthandPropertyTests
{
    [TestMethod]
    public void CssFontShorthandWithFractionLegal()
    {
        var property = ParseDeclaration("font : 12px/14px sans-serif ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("12px / 14px sans-serif", concrete.Value);
    }

    [TestMethod]
    public void CssFontShorthandPercentLegal()
    {
        var property = ParseDeclaration("font : 80% sans-serif ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("80% sans-serif", concrete.Value);
    }

    [TestMethod]
    public void CssFontShorthandBoldItalicLargeLegal()
    {
        // The WithAny(style, variant, weight, stretch) group re-serializes in its declared canonical order
        // (style, variant, weight, stretch) regardless of input order - "bold italic" round-trips as "italic bold".
        var property = ParseDeclaration("font : bold italic large serif ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("italic bold large serif", concrete.Value);
    }

    [TestMethod]
    public void CssFontShorthandPredefinedLegal()
    {
        var property = ParseDeclaration("font : status-bar ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("status-bar", concrete.Value);
    }

    [TestMethod]
    public void CssFontShorthandSizeAndFontListLegal()
    {
        var property = ParseDeclaration("font : 15px arial,sans-serif ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("15px arial, sans-serif", concrete.Value);
    }

    [TestMethod]
    public void CssFontShorthandStyleWeightSizeLineHeightAndFontListLegal()
    {
        var property = ParseDeclaration("font : italic bold 12px/30px Georgia, serif");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("italic bold 12px / 30px Georgia, serif", concrete.Value);
    }

    [TestMethod]
    public void CssFontSizeHeightFamilyLegal()
    {
        var property = ParseDeclaration("font: 12pt/14pt sans-serif ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("12pt / 14pt sans-serif", concrete.Value);
    }

    [TestMethod]
    public void CssFontSizeFamilyLegal()
    {
        var property = ParseDeclaration("font: 80% sans-serif ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("80% sans-serif", concrete.Value);
    }

    [TestMethod]
    public void CssFontSizeHeightMultipleFamiliesLegal()
    {
        // Single-quoted family name input round-trips through the string converter as a double-quoted
        // literal (StringValueConverter.CssText -> string.StylesheetString()).
        var property = ParseDeclaration("font: x-large/110% 'New Century Schoolbook', serif ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("x-large / 110% \"New Century Schoolbook\", serif", concrete.Value);
    }

    [TestMethod]
    public void CssFontWeightVariantSizeFamiliesLegal()
    {
        var property = ParseDeclaration("font: bold italic large Palatino, serif ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("italic bold large Palatino, serif", concrete.Value);
    }

    [TestMethod]
    public void CssFontStyleVariantSizeHeightFamilyLegal()
    {
        // A generic family keyword (here "Fantasy") is stored and re-serialized as the literal lowercased
        // keyword, not resolved to a concrete face - Converters.DefaultFontFamiliesConverter is a
        // DictionaryValueConverter, whose EnumeratedValue.CssText is the matched identifier itself.
        var property = ParseDeclaration("font: normal small-caps 120%/120% Fantasy ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("normal small-caps 120% / 120% fantasy", concrete.Value);
    }

    [TestMethod]
    public void CssFontStyleVariantSizeFamiliesLegal()
    {
        // font-stretch ("condensed") is part of the font shorthand's WithAny(style, variant, weight, stretch)
        // group here too, matching PeachPDF.
        var property = ParseDeclaration("font: condensed oblique 12pt \"Helvetica Neue\", serif ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("oblique condensed 12pt \"Helvetica Neue\", serif", concrete.Value);
    }

    [TestMethod]
    public void CssFontSystemFamilyLegal()
    {
        var property = ParseDeclaration("font: status-bar ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("status-bar", concrete.Value);
    }

    [TestMethod]
    public void CssFontInitialAll()
    {
        var property = ParseDeclaration("font: initial ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("initial", concrete.Value);
    }

    [TestMethod]
    public void CssFontInheritAll()
    {
        var property = ParseDeclaration("font: inherit ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("inherit", concrete.Value);
    }

    [TestMethod]
    public void CssFontUnsetAll()
    {
        var property = ParseDeclaration("font: unset ");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("unset", concrete.Value);
    }

    [TestMethod]
    public void CssFontStyleWeightSizeHeightFamiliesLegal()
    {
        var property = ParseDeclaration("font: italic bold 12px/30px Georgia, serif");
        Assert.AreEqual("font", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontProperty>(property);
        var concrete = (FontProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("italic bold 12px / 30px Georgia, serif", concrete.Value);
    }
}
