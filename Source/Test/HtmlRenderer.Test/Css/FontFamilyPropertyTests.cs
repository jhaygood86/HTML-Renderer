using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/FontProperty.cs (source class <c>CssFontPropertyTests</c>), the
/// <c>font-family</c> cases only. Exercises <see cref="FontFamilyProperty"/>
/// (Source/HtmlRenderer/Core/CssEngine/StyleProperties/Font/FontFamilyProperty.cs), whose converter is
/// <c>Converters.FontFamiliesConverter</c> = <c>DefaultFontFamiliesConverter.Or(StringConverter).Or(LiteralsConverter).FromList()</c>
/// (Source/HtmlRenderer/Core/CssEngine/Model/Converters.cs). Each comma-separated family in the list must be
/// either a generic keyword recognized by <c>Map.DefaultFontFamilies</c> (serif/sans-serif/monospace/
/// cursive/fantasy - Map.cs), a quoted &lt;string&gt;, or a whitespace-joined run of bare &lt;ident&gt; tokens
/// (<c>ValueExtensions.ToLiterals</c>) - never a mix of a string and a bare ident in the same entry, and never
/// a numeric/hash/at/delimiter token anywhere in an unquoted entry.
/// </summary>
[TestClass]
public sealed class FontFamilyPropertyTests
{
    [TestMethod]
    public void CssFontFamilyMultipleWithIdentifiersLegal()
    {
        var property = ParseDeclaration("font-family: Gill Sans Extrabold, sans-serif ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("Gill Sans Extrabold, sans-serif", concrete.Value);
    }

    [TestMethod]
    public void CssFontFamilyInitialLegal()
    {
        var property = ParseDeclaration("font-family: initial ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("initial", concrete.Value);
    }

    [TestMethod]
    public void CssFontFamilyMultipleDiverseLegal()
    {
        var property = ParseDeclaration("font-family: Courier, \"Lucida Console\", monospace ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("Courier, \"Lucida Console\", monospace", concrete.Value);
    }

    [TestMethod]
    public void CssFontFamilyMultipleStringLegal()
    {
        var property = ParseDeclaration("font-family: \"Goudy Bookletter 1911\", sans-serif ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("\"Goudy Bookletter 1911\", sans-serif", concrete.Value);
    }

    [TestMethod]
    public void CssFontFamilyMultipleNumberIllegal()
    {
        // "1911" is a Number token, not an Ident - ValueExtensions.ToLiterals requires every token in an
        // unquoted family entry to be an Ident, so the whole list is rejected.
        var property = ParseDeclaration("font-family: Goudy Bookletter 1911, sans-serif  ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssFontFamilyMultipleFractionIllegal()
    {
        // The "/" delimiter between "Red" and "Black" breaks ToLiterals's whitespace-only-between-idents rule.
        var property = ParseDeclaration("font-family: Red/Black, sans-serif  ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssFontFamilyMultipleStringMixedWithIdentifierIllegal()
    {
        // A single family entry can be a string OR a run of idents, never both.
        var property = ParseDeclaration("font-family: \"Lucida\" Grande, sans-serif ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssFontFamilyMultipleExclamationMarkIllegal()
    {
        var property = ParseDeclaration("font-family: Ahem!, sans-serif ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssFontFamilyMultipleAtIllegal()
    {
        var property = ParseDeclaration("font-family: test@foo, sans-serif ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssFontFamilyHashIllegal()
    {
        var property = ParseDeclaration("font-family: #POUND ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssFontFamilyDashIllegal()
    {
        var property = ParseDeclaration("font-family: Hawaii 5-0 ");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontFamilyProperty>(property);
        var concrete = (FontFamilyProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }
}
