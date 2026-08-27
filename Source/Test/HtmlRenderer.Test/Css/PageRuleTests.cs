using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/PageRuleTests.cs.
/// All structural @page tests apply: <see cref="PageRule"/>, <see cref="MarginStyleRule"/>,
/// <see cref="PageSizeProperty"/> (Style.Size), Style.MarginTop, <see cref="PageNameProperty"/>
/// (Style.PageName), and Style.Width all exist with matching shape.
/// Dropped entirely (SKIP, not ported, not [Ignore]d): every test that calls
/// <c>PeachPDF.Html.Core.Parse.DomParser.ParseLengthToPdfPoints(...)</c> or constructs a
/// <c>PageLengthContext</c> - the ParseLengthToPdfPoints_* theories, plus
/// AtPage_FirstPseudoSelector_MarginTopIsParseable and
/// AtPage_FirstPseudoSelector_ZeroMargin_RoundTripsToZeroPoints (which call it after the structural
/// assertion). This is a PDF-page-box length-to-points resolver with no HTML-Renderer counterpart at all
/// (HTML-Renderer isn't a PDF renderer) - confirmed zero hits for "ParseLengthToPdfPoints"/
/// "PageLengthContext" anywhere under Source/HtmlRenderer.
/// </summary>
[TestClass]
public sealed class PageRuleTests
{
    // ── @page { size: ... } ────────────────────────────────────────────────

    [TestMethod]
    public void AtPage_SizeA4_IsStoredOnStyleDeclaration()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page { size: A4; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual("A4", rule.Style.Size);
    }

    [TestMethod]
    public void AtPage_SizeLetter_IsStoredOnStyleDeclaration()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page { size: letter; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual("letter", rule.Style.Size);
    }

    [TestMethod]
    public void AtPage_SizeA4Landscape_IsStoredOnStyleDeclaration()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page { size: A4 landscape; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual("A4 landscape", rule.Style.Size);
    }

    [TestMethod]
    public void AtPage_SizeExplicitDimensions_IsStoredOnStyleDeclaration()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page { size: 210mm 297mm; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual("210mm 297mm", rule.Style.Size);
    }

    // ── @page pseudo-selectors ─────────────────────────────────────────────

    [TestMethod]
    public void AtPage_NoSelector_HasNullSelector()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page { margin: 20mm; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.IsNull(rule.Selector);
    }

    [TestMethod]
    public void AtPage_FirstPseudoSelector_SelectorTextIsFirst()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page :first { margin-top: 0; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual(":first", rule.SelectorText);
    }

    [TestMethod]
    public void AtPage_LeftPseudoSelector_SelectorTextIsLeft()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page :left { margin-left: 30mm; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual(":left", rule.SelectorText);
    }

    [TestMethod]
    public void AtPage_RightPseudoSelector_SelectorTextIsRight()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page :right { margin-right: 30mm; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual(":right", rule.SelectorText);
    }

    // ── @page named selectors ──────────────────────────────────────────────
    // Regression coverage: a bare identifier selector used to make the whole rule fail to parse in
    // PeachPDF (CreatePageSelector only handled the leading-colon pseudo-class case), so
    // "@page chapter { }" silently vanished instead of producing a PageRule at all.

    [TestMethod]
    public void AtPage_NamedSelector_SelectorTextIsName()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page chapter { margin-top: 0; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual("chapter", rule.SelectorText);
    }

    [TestMethod]
    public void AtPage_NamedSelector_IsNotColonPrefixed()
    {
        // Distinguishes a named-page selector from a pseudo-class one: SelectPageRule's matching
        // logic branches on whether the selector text starts with ':'.
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page chapter { margin-top: 0; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.IsFalse(rule.SelectorText.StartsWith(':'));
    }

    [TestMethod]
    public void AtPage_CommaSeparatedNamedSelectors_AllNamesPresent()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page chapter1, chapter2, chapter3 { margin-top: 0; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual("chapter1, chapter2, chapter3", rule.SelectorText);
    }

    [TestMethod]
    public void AtPage_NamedSelector_PreservesCase()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page Chapter { margin-top: 0; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual("Chapter", rule.SelectorText);
    }

    // ── @page compound name:pseudo selectors ───────────────────────────────
    // Regression coverage: "@page chapter1:left { }" used to fail to parse entirely in PeachPDF
    // (CreatePageSelector stopped consuming tokens at the first non-comma token after an ident),
    // silently dropping the whole rule.

    [TestMethod]
    public void AtPage_CompoundNamePseudoSelector_ParsesAsOneRule()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page chapter1:left { margin-top: 0; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual("chapter1:left", rule.SelectorText);
    }

    [TestMethod]
    public void AtPage_CompoundCommaSeparatedSelectors_AllEntriesPresent()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page chapter1:left, chapter2:left { margin-top: 0; }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual("chapter1:left, chapter2:left", rule.SelectorText);
    }

    // ── @page margin boxes ─────────────────────────────────────────────────

    [TestMethod]
    public void AtPage_TopCenterMarginBox_IsParsedIntoMargins()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page { @top-center { content: \"Page\"; } }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        var margin = rule.Margins.FirstOrDefault();
        Assert.IsNotNull(margin);
    }

    [TestMethod]
    public void AtPage_TopCenterMarginBox_HasTopCenterSelector()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page { @top-center { content: counter(page); } }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        var margin = rule.Margins.FirstOrDefault(m => m.Selector?.Text?.Contains("top-center") == true);
        Assert.IsNotNull(margin);
    }

    [TestMethod]
    public void AtPage_TopCenterMarginBox_ContentIsNonEmpty()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page { @top-center { content: \"Header\"; } }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        var margin = rule.Margins.FirstOrDefault();
        Assert.IsNotNull(margin);
        Assert.IsFalse(string.IsNullOrEmpty(margin.Style.Content));
    }

    [TestMethod]
    public void AtPage_MultipleMarginBoxes_AllParsed()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet(@"
            @page {
                @top-left   { content: ""Left""; }
                @top-center { content: ""Center""; }
                @top-right  { content: ""Right""; }
            }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        Assert.AreEqual(3, rule.Margins.Count());
    }

    // ── named pages ────────────────────────────────────────────────────────

    [TestMethod]
    public void PageNameProperty_Identifier_ParsesCorrectly()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("div { page: chapter; }");
        var block = sheet.Rules.OfType<StyleRule>().FirstOrDefault();
        Assert.IsNotNull(block);
        Assert.AreEqual("chapter", block.Style.PageName);
    }

    [TestMethod]
    public void PageNameProperty_Auto_ParsesCorrectly()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("div { page: auto; }");
        var block = sheet.Rules.OfType<StyleRule>().FirstOrDefault();
        Assert.IsNotNull(block);
        Assert.AreEqual("auto", block.Style.PageName);
    }

    // ── margin box explicit width ───────────────────────────────────────────

    [TestMethod]
    public void AtPage_MarginBox_ExplicitWidthIsParsedFromStyle()
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet("@page { @top-left { content: \"L\"; width: 100pt; } }");
        var rule = sheet.Rules.OfType<PageRule>().FirstOrDefault();
        Assert.IsNotNull(rule);
        var margin = rule.Margins.FirstOrDefault(m => m.Selector?.Text?.Contains("top-left") == true);
        Assert.IsNotNull(margin);
        Assert.AreEqual("100pt", margin.Style.Width);
    }
}
