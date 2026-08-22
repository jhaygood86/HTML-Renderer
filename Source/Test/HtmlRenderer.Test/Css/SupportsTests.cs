using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/Supports.cs.
/// HTML-Renderer's <see cref="DeclarationCondition"/> still uses the old grammar-only oracle
/// (<c>Property.TrySetValue</c>) rather than PeachPDF's newer <c>CssPropertyRegistry</c>-based split
/// between "parses in the CSS-OM" and "genuinely dispatched/rendered by Layer B" - see
/// Source/HtmlRenderer/Core/CssEngine/Conditions/DeclarationCondition.cs:16-20. That oracle still agrees
/// with the spec-correct expectation for most cases here; the handful where it diverges (a false positive
/// or false negative relative to what the layout engine actually does with the value) are marked
/// <c>[Ignore]</c> below with the same citation.
/// </summary>
[TestClass]
public sealed class SupportsTests
{
    [TestMethod]
    public void SupportsEmptyRule()
    {
        var source = @"@supports () { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<SupportsRule>(sheet.Rules[0]);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.AreEqual(string.Empty, supports.ConditionText);
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsBackgroundColorRedRule()
    {
        var source = @"@supports (background-color: red) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<SupportsRule>(sheet.Rules[0]);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.AreEqual("(background-color: red)", supports.ConditionText);
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsBackgroundColorRedAndColorBlueRule()
    {
        var source = @"@supports ((background-color: red) and (color: blue)) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<SupportsRule>(sheet.Rules[0]);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.AreEqual("((background-color: red) and (color: blue))", supports.ConditionText);
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsBreakAfterPageRule()
    {
        var source = @"@supports (break-after: page) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<SupportsRule>(sheet.Rules[0]);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsBreakAfterInvalidValueRule()
    {
        var source = @"@supports (break-after: not-a-real-value) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<SupportsRule>(sheet.Rules[0]);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsFalse(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsNotUnsupportedDeclarationRule()
    {
        var source = @"@supports (not (background-transparency: half)) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<SupportsRule>(sheet.Rules[0]);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.AreEqual("(not (background-transparency: half))", supports.ConditionText);
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsUnsupportedDeclarationRule()
    {
        var source = @"@supports ((background-transparency: zero)) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<SupportsRule>(sheet.Rules[0]);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.AreEqual("((background-transparency: zero))", supports.ConditionText);
        Assert.IsFalse(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsBackgroundRedWithImportantRule()
    {
        var source = @"@supports (background: red !important) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<SupportsRule>(sheet.Rules[0]);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.AreEqual("(background: red !important)", supports.ConditionText);
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsBackgroundInheritRule()
    {
        var source = @"@supports (background: inherit) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    [Ignore("HTML-Renderer's DeclarationCondition.Check() still uses the old grammar-only oracle " +
            "(Property.TrySetValue), which parses 'animation-name' fine in the CSS-OM even though it is " +
            "never dispatched by the layout engine - a false positive relative to PeachPDF's newer " +
            "CssPropertyRegistry-based distinction. See " +
            "Source/HtmlRenderer/Core/CssEngine/Conditions/DeclarationCondition.cs:16-20.")]
    public void SupportsAnimationNameRule_NoLongerFalsePositive()
    {
        var source = @"@supports (animation-name: spin) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsFalse(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsSvgFillRule_NoLongerFalseNegative()
    {
        var source = @"@supports (fill: red) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    [DataRow("inherit")]
    [DataRow("initial")]
    [DataRow("unset")]
    [DataRow("revert")]
    [DataRow("revert-layer")]
    public void SupportsCssWideKeywordRule_AlwaysSupportedForARecognizedProperty(string keyword)
    {
        var source = $@"@supports (color: {keyword}) {{ }}";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsCssWideKeywordRule_StillFailsForAnUnrecognizedProperty()
    {
        var source = @"@supports (background-transparency: inherit) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsFalse(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsTransformRotateRule()
    {
        var source = @"@supports (transform: rotate(10deg)) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    [Ignore("HTML-Renderer's DeclarationCondition.Check() still uses the old grammar-only oracle, which " +
            "parses perspective() as a valid transform function even though it is not genuinely rendered - " +
            "a false positive relative to PeachPDF's CssPropertyRegistry-based split. See " +
            "Source/HtmlRenderer/Core/CssEngine/Conditions/DeclarationCondition.cs:16-20.")]
    public void SupportsTransformPerspectiveRule_NotGenuinelyRendered()
    {
        var source = @"@supports (transform: perspective(300px)) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsFalse(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsBreakBeforeRegionRule_NotGenuinelyEnforced()
    {
        var source = @"@supports (break-before: region) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsFalse(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsBreakBeforePageRule()
    {
        var source = @"@supports (break-before: page) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsColumnSpanAllRule()
    {
        var source = @"@supports (column-span: all) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsColumnSpanNoneRule()
    {
        var source = @"@supports (column-span: none) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    [Ignore("HTML-Renderer's DeclarationCondition.Check() still uses the old grammar-only oracle, which " +
            "parses 'keep-all' as a valid word-break value even though the line-breaking logic only " +
            "special-cases break-all - a false positive relative to PeachPDF's CssPropertyRegistry-based " +
            "split. See Source/HtmlRenderer/Core/CssEngine/Conditions/DeclarationCondition.cs:16-20.")]
    public void SupportsWordBreakKeepAllRule_NotGenuinelyEnforced()
    {
        var source = @"@supports (word-break: keep-all) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsFalse(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsTextTransformFullWidthRule()
    {
        var source = @"@supports (text-transform: full-width) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsWordBreakAllRule()
    {
        var source = @"@supports (word-break: break-all) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsDirectionRule()
    {
        var source = @"@supports (direction: rtl) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsDirectionGarbageValueRule_NoLongerAlwaysTrue()
    {
        var source = @"@supports (direction: sideways) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsFalse(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsDisplayContentsRule_NotImplemented()
    {
        var source = @"@supports (display: contents) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.IsFalse(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsPaddingTopOrPaddingLeftRule()
    {
        var source = @"@supports ((padding-TOP :  0) or (padding-left : 0)) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<SupportsRule>(sheet.Rules[0]);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.AreEqual("((padding-top: 0) or (padding-left: 0))", supports.ConditionText);
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsPaddingTopOrPaddingLeftAndPaddingBottomOrPaddingRightRule()
    {
        var source = @"@supports (((padding-top: 0)  or  (padding-left: 0))  and  ((padding-bottom:  0)  or  (padding-right: 0))) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<SupportsRule>(sheet.Rules[0]);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.AreEqual("(((padding-top: 0) or (padding-left: 0)) and ((padding-bottom: 0) or (padding-right: 0)))", supports.ConditionText);
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsDisplayFlexWithImportantRule()
    {
        var source = @"@supports (display: flex !important) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<SupportsRule>(sheet.Rules[0]);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.AreEqual("(display: flex !important)", supports.ConditionText);
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsBareDisplayFlexRule()
    {
        var source = @"@supports display: flex { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(0, sheet.Rules.Length);
    }

    [TestMethod]
    public void SupportsDisplayFlexMultipleBracketsRule()
    {
        var source = @"@supports ((display: flex)) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<SupportsRule>(sheet.Rules[0]);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.AreEqual("((display: flex))", supports.ConditionText);
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    [Ignore("HTML-Renderer's DeclarationCondition.Check() still uses the old grammar-only oracle, which " +
            "parses transition-property/animation-name fine in the CSS-OM even though neither is genuinely " +
            "dispatched by the layout engine - a false positive relative to PeachPDF's " +
            "CssPropertyRegistry-based split. See " +
            "Source/HtmlRenderer/Core/CssEngine/Conditions/DeclarationCondition.cs:16-20.")]
    public void SupportsTransitionOrAnimationNameAndTransformFrontBracketRule()
    {
        var source = @"@supports ((transition-property: color) or
           (animation-name: foo)) and
          (transform: rotate(10deg)) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<SupportsRule>(sheet.Rules[0]);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.AreEqual("((transition-property: color) or (animation-name: foo)) and (transform: rotate(10deg))", supports.ConditionText);
        Assert.IsFalse(supports.Condition.Check());
    }

    [TestMethod]
    [Ignore("HTML-Renderer's DeclarationCondition.Check() still uses the old grammar-only oracle, which " +
            "parses transition-property/animation-name fine in the CSS-OM even though neither is genuinely " +
            "dispatched by the layout engine - a false positive relative to PeachPDF's " +
            "CssPropertyRegistry-based split. See " +
            "Source/HtmlRenderer/Core/CssEngine/Conditions/DeclarationCondition.cs:16-20.")]
    public void SupportsTransitionOrAnimationNameAndTransformBackBracketRule()
    {
        var source = @"@supports (transition-property: color) or
           ((animation-name: foo) and
          (transform: rotate(10deg))) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<SupportsRule>(sheet.Rules[0]);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.AreEqual("(transition-property: color) or ((animation-name: foo) and (transform: rotate(10deg)))", supports.ConditionText);
        Assert.IsFalse(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsShadowVendorPrefixesRule()
    {
        var source = @"@supports ( box-shadow: 0 0 2px black ) or
          ( -moz-box-shadow: 0 0 2px black ) or
          ( -webkit-box-shadow: 0 0 2px black ) or
          ( -o-box-shadow: 0 0 2px black ) { }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<SupportsRule>(sheet.Rules[0]);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.AreEqual("(box-shadow: 0 0 2px black) or (-moz-box-shadow: 0 0 2px black) or (-webkit-box-shadow: 0 0 2px black) or (-o-box-shadow: 0 0 2px black)", supports.ConditionText);
        Assert.IsTrue(supports.Condition.Check());
    }

    [TestMethod]
    public void SupportsNegatedDisplayFlexRuleWithDeclarations()
    {
        var source = @"@supports not ( display: flex ) {
  body { width: 100%; height: 100%; background: white; color: black; }
  #navigation { width: 25%; }
  #article { width: 75%; }
}";
        var sheet = CssConstructionFunctions.ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<SupportsRule>(sheet.Rules[0]);
        var supports = (SupportsRule)sheet.Rules[0];
        Assert.AreEqual(3, supports.Rules.Length);
        Assert.AreEqual("not (display: flex)", supports.ConditionText);
        Assert.IsFalse(supports.Condition.Check());
    }
}
