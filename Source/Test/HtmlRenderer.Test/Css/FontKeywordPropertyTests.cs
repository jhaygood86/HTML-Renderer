using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/FontProperty.cs (source class <c>CssFontPropertyTests</c>), the
/// standalone font-related keyword/value longhands: <c>font-variant</c> (base css2 grammar only -
/// see the class remarks below), <c>font-style</c>, <c>font-size</c>, <c>font-weight</c>, <c>font-stretch</c>,
/// <c>letter-spacing</c>, and <c>font-size-adjust</c>. All under
/// Source/HtmlRenderer/Core/CssEngine/StyleProperties/Font/.
///
/// Not ported: PeachPDF's <c>font-variant-ligatures</c>, <c>font-variant-caps</c>, <c>font-variant-numeric</c>,
/// <c>font-variant-east-asian</c>, and <c>font-feature-settings</c> tests - none of those properties exist in
/// this fork (confirmed: no matching type under StyleProperties/Font/, no <c>PropertyNames</c> entries). For
/// the same reason, <c>Converters.FontVariantConverter</c> = <c>Map.FontVariants.ToConverter()</c>
/// (Converters.cs) only recognizes the original CSS2 two-keyword grammar - <c>Map.FontVariants</c>
/// (Model/Map.cs) maps just "normal" and "small-caps", not the CSS Fonts 4 <c>font-variant-caps</c>
/// seven-keyword grammar PeachPDF's own font-variant shorthand now also accepts standalone. So PeachPDF's
/// CssFontVariantNoneLegal, CssFontVariantCombinesCapsAndLigaturesAxesLegal, and
/// CssFontVariantShorthandAcceptsPreviouslyRejectedCapsKeywords (all-small-caps/petite-caps/etc.) are skipped
/// too - they'd need font-variant-caps/-ligatures support this fork doesn't have.
/// </summary>
[TestClass]
public sealed class FontKeywordPropertyTests
{
    // ── font-variant (base keywords only) ────────────────────────────────────────────────────────────────

    [TestMethod]
    public void CssFontVariantNormalUppercaseLegal()
    {
        var property = ParseDeclaration("font-variant : NORMAL");
        Assert.AreEqual("font-variant", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontVariantProperty>(property);
        var concrete = (FontVariantProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("normal", concrete.Value);
    }

    [TestMethod]
    public void CssFontVariantSmallCapsLegal()
    {
        var property = ParseDeclaration("font-variant : small-caps ");
        Assert.AreEqual("font-variant", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontVariantProperty>(property);
        var concrete = (FontVariantProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("small-caps", concrete.Value);
    }

    [TestMethod]
    public void CssFontVariantSmallCapsIllegal()
    {
        var property = ParseDeclaration("font-variant : smallCaps ");
        Assert.AreEqual("font-variant", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontVariantProperty>(property);
        var concrete = (FontVariantProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    // ── font-style ────────────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    public void CssFontStyleItalicLegal()
    {
        var property = ParseDeclaration("font-style : italic");
        Assert.AreEqual("font-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontStyleProperty>(property);
        var concrete = (FontStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("italic", concrete.Value);
    }

    [TestMethod]
    public void CssFontStyleObliqueLegal()
    {
        var property = ParseDeclaration("font-style : oblique ");
        Assert.AreEqual("font-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontStyleProperty>(property);
        var concrete = (FontStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("oblique", concrete.Value);
    }

    [TestMethod]
    public void CssFontStyleObliqueWithAngleLegal()
    {
        // CSS Fonts 4's "oblique <angle>" form - Converters.FontStyleConverter's second branch
        // (Model/Converters.cs), a StartsWithValueConverter over AngleConverter.
        var property = ParseDeclaration("font-style : oblique 10deg");
        Assert.AreEqual("font-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontStyleProperty>(property);
        var concrete = (FontStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("oblique 10deg", concrete.Value);
    }

    [TestMethod]
    public void CssFontStyleObliqueWithNegativeAngleLegal()
    {
        var property = ParseDeclaration("font-style : oblique -10deg");
        Assert.AreEqual("font-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontStyleProperty>(property);
        var concrete = (FontStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("oblique -10deg", concrete.Value);
    }

    [TestMethod]
    public void CssFontStyleNormalImportantLegal()
    {
        var property = ParseDeclaration("font-style : normal !important");
        Assert.AreEqual("font-style", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.IsInstanceOfType<FontStyleProperty>(property);
        var concrete = (FontStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("normal", concrete.Value);
    }

    // ── font-size ─────────────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    public void CssFontSizeAbsoluteImportantXxSmallLegal()
    {
        var property = ParseDeclaration("font-size : xx-small !important");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeProperty>(property);
        var concrete = (FontSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("xx-small", concrete.Value);
    }

    [TestMethod]
    public void CssFontSizeAbsoluteMediumUppercaseLegal()
    {
        var property = ParseDeclaration("font-size : medium");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeProperty>(property);
        var concrete = (FontSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("medium", concrete.Value);
    }

    [TestMethod]
    public void CssFontSizeAbsoluteLargeImportantLegal()
    {
        var property = ParseDeclaration("font-size : large !important");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeProperty>(property);
        var concrete = (FontSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("large", concrete.Value);
    }

    [TestMethod]
    public void CssFontSizeRelativeLargerLegal()
    {
        var property = ParseDeclaration("font-size : larger ");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeProperty>(property);
        var concrete = (FontSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("larger", concrete.Value);
    }

    [TestMethod]
    public void CssFontSizeRelativeLargestIllegal()
    {
        // "largest" isn't a real CSS keyword (the relative pair is larger/smaller) - not in Map.FontSizes.
        var property = ParseDeclaration("font-size : largest ");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeProperty>(property);
        var concrete = (FontSizeProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssFontSizePercentLegal()
    {
        var property = ParseDeclaration("font-size : 120% ");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeProperty>(property);
        var concrete = (FontSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("120%", concrete.Value);
    }

    [TestMethod]
    public void CssFontSizeZeroLegal()
    {
        var property = ParseDeclaration("font-size : 0 ");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeProperty>(property);
        var concrete = (FontSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0", concrete.Value);
    }

    [TestMethod]
    public void CssFontSizeLengthLegal()
    {
        var property = ParseDeclaration("font-size : 3.5em ");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeProperty>(property);
        var concrete = (FontSizeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("3.5em", concrete.Value);
    }

    [TestMethod]
    public void CssFontSizeNumberIllegal()
    {
        // A bare non-zero number has no unit and isn't a percentage - font-size doesn't treat unitless
        // numbers specially (that's line-height's job), so it's rejected.
        var property = ParseDeclaration("font-size : 120.3 ");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeProperty>(property);
        var concrete = (FontSizeProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    // ── font-weight ───────────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    public void CssFontWeightPercentllegal()
    {
        var property = ParseDeclaration("font-weight : 100% ");
        Assert.AreEqual("font-weight", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontWeightProperty>(property);
        var concrete = (FontWeightProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssFontWeightBolderLegalImportant()
    {
        var property = ParseDeclaration("font-weight : bolder !important");
        Assert.AreEqual("font-weight", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.IsInstanceOfType<FontWeightProperty>(property);
        var concrete = (FontWeightProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("bolder", concrete.Value);
    }

    [TestMethod]
    public void CssFontWeightBoldLegal()
    {
        var property = ParseDeclaration("font-weight : bold");
        Assert.AreEqual("font-weight", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontWeightProperty>(property);
        var concrete = (FontWeightProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("bold", concrete.Value);
    }

    [TestMethod]
    public void CssFontWeight400Legal()
    {
        var property = ParseDeclaration("font-weight : 400 ");
        Assert.AreEqual("font-weight", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontWeightProperty>(property);
        var concrete = (FontWeightProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("400", concrete.Value);
    }

    [TestMethod]
    [Ignore("This fork's font-weight grammar hasn't picked up CSS Fonts 4's <number [1,1000]> - " +
            "ValueExtensions.ToWeightInteger (Extensions/ValueExtensions.cs) delegates to the private " +
            "IsWeight(int) helper, which only accepts the nine exact multiples of 100 from 100 to 900 " +
            "(the old CSS2.1 grammar), not an arbitrary integer in [1,1000]. PeachPDF's own port already " +
            "closed this gap (issue #655, matching its CssFontWeight550Legal); this fork hasn't yet, so " +
            "550 is currently rejected rather than accepted.")]
    public void CssFontWeight550Legal()
    {
        var property = ParseDeclaration("font-weight : 550 ");
        Assert.AreEqual("font-weight", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontWeightProperty>(property);
        var concrete = (FontWeightProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("550", concrete.Value);
    }

    [TestMethod]
    public void CssFontWeight550IsCurrentlyIllegal()
    {
        // Mirrors CSS Fonts 4's font-weight: 550 (see the [Ignore]d CssFontWeight550Legal above) - documents
        // this fork's actual current behavior: rejected, because IsWeight only allows multiples of 100.
        var property = ParseDeclaration("font-weight : 550 ");
        Assert.AreEqual("font-weight", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontWeightProperty>(property);
        var concrete = (FontWeightProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssFontWeightOutOfRangeIllegal()
    {
        var property = ParseDeclaration("font-weight : 1001 ");
        Assert.AreEqual("font-weight", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontWeightProperty>(property);
        var concrete = (FontWeightProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssFontWeightZeroIllegal()
    {
        var property = ParseDeclaration("font-weight : 0 ");
        Assert.AreEqual("font-weight", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontWeightProperty>(property);
        var concrete = (FontWeightProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    // ── font-stretch ──────────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    public void CssFontStretchNormalUppercaseImportantLegal()
    {
        var property = ParseDeclaration("font-stretch : NORMAL !important");
        Assert.AreEqual("font-stretch", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.IsInstanceOfType<FontStretchProperty>(property);
        var concrete = (FontStretchProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("normal", concrete.Value);
    }

    [TestMethod]
    public void CssFontStretchExtraCondensedLegal()
    {
        var property = ParseDeclaration("font-stretch : extra-condensed ");
        Assert.AreEqual("font-stretch", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontStretchProperty>(property);
        var concrete = (FontStretchProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("extra-condensed", concrete.Value);
    }

    [TestMethod]
    public void CssFontStretchSemiExpandedSpaceBetweenIllegal()
    {
        // "semi-expanded" must be a single ident token - the space breaks it into two tokens, and
        // Map.FontStretches.ToConverter() (a DictionaryValueConverter) only matches a lone identifier.
        var property = ParseDeclaration("font-stretch : semi expanded ");
        Assert.AreEqual("font-stretch", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontStretchProperty>(property);
        var concrete = (FontStretchProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    // ── letter-spacing ────────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    public void CssLetterSpacingLengthPxLegal()
    {
        var property = ParseDeclaration("letter-spacing: 3px ");
        Assert.AreEqual("letter-spacing", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<LetterSpacingProperty>(property);
        var concrete = (LetterSpacingProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("3px", concrete.Value);
    }

    [TestMethod]
    public void CssLetterSpacingLengthFloatPxLegal()
    {
        var property = ParseDeclaration("letter-spacing: .3px ");
        Assert.AreEqual("letter-spacing", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<LetterSpacingProperty>(property);
        var concrete = (LetterSpacingProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0.3px", concrete.Value);
    }

    [TestMethod]
    public void CssLetterSpacingLengthFloatEmLegal()
    {
        var property = ParseDeclaration("letter-spacing: 0.3em ");
        Assert.AreEqual("letter-spacing", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<LetterSpacingProperty>(property);
        var concrete = (LetterSpacingProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0.3em", concrete.Value);
    }

    [TestMethod]
    public void CssLetterSpacingNormalLegal()
    {
        var property = ParseDeclaration("letter-spacing: normal ");
        Assert.AreEqual("letter-spacing", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<LetterSpacingProperty>(property);
        var concrete = (LetterSpacingProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("normal", concrete.Value);
    }

    // ── font-size-adjust ──────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    public void CssFontSizeAdjustNoneLegal()
    {
        var property = ParseDeclaration("font-size-adjust : NONE");
        Assert.AreEqual("font-size-adjust", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeAdjustProperty>(property);
        var concrete = (FontSizeAdjustProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    public void CssFontSizeAdjustNumberLegal()
    {
        var property = ParseDeclaration("font-size-adjust : 0.5");
        Assert.AreEqual("font-size-adjust", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeAdjustProperty>(property);
        var concrete = (FontSizeAdjustProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0.5", concrete.Value);
    }

    [TestMethod]
    public void CssFontSizeAdjustLengthIllegal()
    {
        // font-size-adjust : Converters.OptionalNumberConverter.OrDefault() - a bare <number> or "none",
        // never a <length>.
        var property = ParseDeclaration("font-size-adjust : 1.1em ");
        Assert.AreEqual("font-size-adjust", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<FontSizeAdjustProperty>(property);
        var concrete = (FontSizeAdjustProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }
}
