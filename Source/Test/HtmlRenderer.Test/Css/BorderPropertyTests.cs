using System.Collections.Generic;
using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/BorderProperty.cs.
/// PeachPDF.CSS models each border-related declaration as a typed, validating Property (HasValue reports
/// whether the value was legal, and illegal/wrong-count values are rejected outright). HTML-Renderer has no
/// such typed property model:
///   * Longhand properties (e.g. "border-top-color", "border-style") and the per-axis shorthands
///     ("border-color", "border-style", "border-width", "border-spacing") are expanded by
///     <see cref="CssParser"/>'s private AddProperty/SplitMultiDirectionValues into a flat
///     Dictionary&lt;string, string&gt; of longhand keys with NO legality validation of the individual
///     tokens -- illegal keywords (e.g. "wavy") are stored verbatim, while a value with more tokens than
///     SplitMultiDirectionValues understands (5 values for a 1/2/3/4-value shorthand) silently leaves all of
///     the corresponding longhand keys unset (a real, testable no-op) rather than being flagged as rejected.
///   * The single-side shorthands ("border", "border-left", "border-top", "border-right", "border-bottom")
///     all funnel through the public <see cref="CssParser.ParseBorder"/>, which tokenizes the value on
///     whitespace and independently matches a width/style/color out of the tokens. Because the tokenizer is
///     whitespace-only, values containing a color function with internal spaces (e.g. "rgb(255, 100, 0)")
///     cannot be recognized as a color at all -- a real, testable current limitation.
/// These tests exercise the real CssData/CssParser/CssBox pipeline and assert on the values it actually
/// produces; a handful of cases where PeachPDF's expectation reflects behavior HTML-Renderer does not (yet)
/// implement are marked with an [Ignore] documenting the intended target behavior.
/// </summary>
[TestClass]
public sealed class BorderPropertyTests
{
    /// <summary>Parses a single declaration inside a throwaway ruleset and returns its expanded, longhand properties.</summary>
    private static IDictionary<string, string> ParseDeclaration(string declaration)
    {
        var cssData = CssData.Parse(new MockAdapter(), $"test {{ {declaration} }}", false);
        return cssData.GetCssBlock("test").First().Properties;
    }

    /// <summary>Parses a "border"/"border-left"/"border-top"/"border-right"/"border-bottom" shorthand value directly.</summary>
    private static void ParseBorderShorthand(string value, out string width, out string style, out string color)
    {
        new CssParser(new MockAdapter()).ParseBorder(value, out width, out style, out color);
    }

    private static RColor ResolveColor(string colorText)
    {
        return new CssParser(new MockAdapter()).ParseColor(colorText);
    }

    // ── border-spacing ───────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    public void BorderSpacingLengthLegal()
    {
        var properties = ParseDeclaration("border-spacing: 20px");

        Assert.AreEqual("20px", properties["border-spacing"]);
    }

    [TestMethod]
    public void BorderSpacingZeroLegal()
    {
        var properties = ParseDeclaration("border-spacing: 0");

        Assert.AreEqual("0", properties["border-spacing"]);
    }

    [TestMethod]
    public void BorderSpacingLengthLengthLegal()
    {
        var properties = ParseDeclaration("border-spacing: 15px 3em");

        Assert.AreEqual("15px 3em", properties["border-spacing"]);
    }

    [TestMethod]
    public void BorderSpacingLengthZeroLegal()
    {
        var properties = ParseDeclaration("border-spacing: 15px 0");

        Assert.AreEqual("15px 0", properties["border-spacing"]);
    }

    [TestMethod]
    [Ignore("not yet spec compliant")]
    public void BorderSpacingPercentIllegal()
    {
        // PeachPDF: a percentage is not a legal border-spacing value, so it is rejected (HasValue == false).
        // HTML-Renderer's CssParser does not dispatch "border-spacing" to any validating parse routine -- it
        // falls through to the unconditional fallback branch in AddProperty and is stored verbatim. Target/
        // intended behavior: the illegal value is rejected and no "border-spacing" property is stored.
        var properties = ParseDeclaration("border-spacing: 15%");

        Assert.IsFalse(properties.ContainsKey("border-spacing"));
    }

    // ── longhand border-*-color ──────────────────────────────────────────────────────────────────────────

    [TestMethod]
    public void BorderBottomColorRedLegal()
    {
        var properties = ParseDeclaration("border-bottom-color: red");

        Assert.AreEqual("red", properties["border-bottom-color"]);
        Assert.AreEqual(RColor.FromArgb(255, 0, 0), ResolveColor(properties["border-bottom-color"]));
    }

    [TestMethod]
    public void BorderTopColorHexLegal()
    {
        var properties = ParseDeclaration("border-top-color: #0F0");

        Assert.AreEqual("#0f0", properties["border-top-color"]);
        Assert.AreEqual(RColor.FromArgb(0, 255, 0), ResolveColor(properties["border-top-color"]));
    }

    [TestMethod]
    public void BorderRightColorRgbaLegal()
    {
        var properties = ParseDeclaration("border-right-color: rgba(1, 1, 1, 0)");

        Assert.AreEqual("rgba(1, 1, 1, 0)", properties["border-right-color"]);
    }

    [TestMethod]
    public void BorderLeftColorRgbLegal()
    {
        var properties = ParseDeclaration("border-left-color: rgb(1, 255, 100)  !important");

        // "!important" is stripped by the parser regardless of the (nonexistent) importance flag
        Assert.AreEqual("rgb(1, 255, 100)", properties["border-left-color"]);
    }

    // ── border-color (all-sides shorthand) ───────────────────────────────────────────────────────────────

    [TestMethod]
    public void BorderColorTransparentLegal()
    {
        var properties = ParseDeclaration("border-color: transparent");

        Assert.AreEqual("transparent", properties["border-top-color"]);
        Assert.AreEqual("transparent", properties["border-right-color"]);
        Assert.AreEqual("transparent", properties["border-bottom-color"]);
        Assert.AreEqual("transparent", properties["border-left-color"]);
    }

    [TestMethod]
    public void BorderColorRedGreenLegal()
    {
        var properties = ParseDeclaration("border-color: red   green");

        Assert.AreEqual("red", properties["border-top-color"]);
        Assert.AreEqual("red", properties["border-bottom-color"]);
        Assert.AreEqual("green", properties["border-left-color"]);
        Assert.AreEqual("green", properties["border-right-color"]);
    }

    [TestMethod]
    public void BorderColorRedRgbLegal()
    {
        var properties = ParseDeclaration("border-color: red   rgb(0,0,0)");

        Assert.AreEqual("red", properties["border-top-color"]);
        Assert.AreEqual("red", properties["border-bottom-color"]);
        Assert.AreEqual("rgb(0,0,0)", properties["border-left-color"]);
        Assert.AreEqual("rgb(0,0,0)", properties["border-right-color"]);
    }

    [TestMethod]
    public void BorderColorRedBlueGreenLegal()
    {
        var properties = ParseDeclaration("border-color: red blue green");

        Assert.AreEqual("red", properties["border-top-color"]);
        Assert.AreEqual("blue", properties["border-left-color"]);
        Assert.AreEqual("blue", properties["border-right-color"]);
        Assert.AreEqual("green", properties["border-bottom-color"]);
    }

    [TestMethod]
    public void BorderColorRedBlueGreenBlackLegal()
    {
        var properties = ParseDeclaration("border-color: red blue green   BLACK");

        Assert.AreEqual("red", properties["border-top-color"]);
        Assert.AreEqual("blue", properties["border-right-color"]);
        Assert.AreEqual("green", properties["border-bottom-color"]);
        Assert.AreEqual("black", properties["border-left-color"]);
    }

    [TestMethod]
    public void BorderColorRedBlueGreenBlackTransparentIllegal()
    {
        // SplitMultiDirectionValues only understands 1-4 values; a 5-value list matches none of its cases,
        // so none of the border-*-color longhands get set at all (a real, silent no-op).
        var properties = ParseDeclaration("border-color: red blue green black transparent");

        Assert.IsFalse(properties.ContainsKey("border-top-color"));
        Assert.IsFalse(properties.ContainsKey("border-right-color"));
        Assert.IsFalse(properties.ContainsKey("border-bottom-color"));
        Assert.IsFalse(properties.ContainsKey("border-left-color"));
    }

    // ── border-style (longhand + all-sides shorthand) ────────────────────────────────────────────────────

    [TestMethod]
    public void BorderStyleDottedLegal()
    {
        var properties = ParseDeclaration("border-style: dotted");

        Assert.AreEqual("dotted", properties["border-top-style"]);
        Assert.AreEqual("dotted", properties["border-right-style"]);
        Assert.AreEqual("dotted", properties["border-bottom-style"]);
        Assert.AreEqual("dotted", properties["border-left-style"]);
    }

    [TestMethod]
    public void BorderStyleInsetOutsetUpperLegal()
    {
        var properties = ParseDeclaration("border-style: INSET   OUTset");

        Assert.AreEqual("inset", properties["border-top-style"]);
        Assert.AreEqual("inset", properties["border-bottom-style"]);
        Assert.AreEqual("outset", properties["border-left-style"]);
        Assert.AreEqual("outset", properties["border-right-style"]);
    }

    [TestMethod]
    public void BorderStyleDoubleGrooveLegal()
    {
        var properties = ParseDeclaration("border-style: double   groove");

        Assert.AreEqual("double", properties["border-top-style"]);
        Assert.AreEqual("double", properties["border-bottom-style"]);
        Assert.AreEqual("groove", properties["border-left-style"]);
        Assert.AreEqual("groove", properties["border-right-style"]);
    }

    [TestMethod]
    public void BorderStyleRidgeSolidDashedLegal()
    {
        var properties = ParseDeclaration("border-style: ridge solid dashed");

        Assert.AreEqual("ridge", properties["border-top-style"]);
        Assert.AreEqual("solid", properties["border-left-style"]);
        Assert.AreEqual("solid", properties["border-right-style"]);
        Assert.AreEqual("dashed", properties["border-bottom-style"]);
    }

    [TestMethod]
    public void BorderStyleHiddenDottedNoneNoneLegal()
    {
        var properties = ParseDeclaration("border-style   :   hidden  dotted  NONE   nONe");

        Assert.AreEqual("hidden", properties["border-top-style"]);
        Assert.AreEqual("dotted", properties["border-right-style"]);
        Assert.AreEqual("none", properties["border-bottom-style"]);
        Assert.AreEqual("none", properties["border-left-style"]);
    }

    [TestMethod]
    [Ignore("not yet spec compliant")]
    public void BorderStyleWavyIllegal()
    {
        // PeachPDF: an invalid border-style keyword is rejected (HasValue == false). HTML-Renderer's
        // SplitMultiDirectionValues-based shorthand expansion performs no keyword legality validation, so
        // "wavy" is currently accepted and stored verbatim on all four sides. Target/intended behavior: the
        // illegal keyword is rejected and no border-*-style longhands are set.
        var properties = ParseDeclaration("border-style: wavy");

        Assert.IsFalse(properties.ContainsKey("border-top-style"));
    }

    [TestMethod]
    public void BorderBottomStyleGrooveLegal()
    {
        var properties = ParseDeclaration("border-bottom-style: GROOVE");

        Assert.AreEqual("groove", properties["border-bottom-style"]);
    }

    [TestMethod]
    public void BorderTopStyleNoneLegal()
    {
        var properties = ParseDeclaration("border-top-style:none");

        Assert.AreEqual("none", properties["border-top-style"]);
    }

    [TestMethod]
    public void BorderRightStyleDoubleLegal()
    {
        var properties = ParseDeclaration("border-right-style:double");

        Assert.AreEqual("double", properties["border-right-style"]);
    }

    [TestMethod]
    public void BorderLeftStyleHiddenLegal()
    {
        var properties = ParseDeclaration("border-left-style: hidden  !important");

        Assert.AreEqual("hidden", properties["border-left-style"]);
    }

    // ── border-width (longhand + all-sides shorthand) ────────────────────────────────────────────────────

    [TestMethod]
    public void BorderBottomWidthThinLegal()
    {
        var properties = ParseDeclaration("border-bottom-width: THIN");

        Assert.AreEqual("thin", properties["border-bottom-width"]);
        // "thin"/"medium"/"thick" resolve to fixed pixel widths independent of any box (HTML-Renderer's own
        // scale: thin=1, medium=2, thick=4 -- different from PeachPDF's 1/3/5 scale).
        Assert.AreEqual(1d, CssValueParser.GetActualBorderWidth("thin", null!));
    }

    [TestMethod]
    public void BorderTopWidthZeroLegal()
    {
        var properties = ParseDeclaration("border-top-width: 0");

        Assert.AreEqual("0", properties["border-top-width"]);
    }

    [TestMethod]
    public void BorderRightWidthEmLegal()
    {
        var properties = ParseDeclaration("border-right-width: 3em");

        Assert.AreEqual("3em", properties["border-right-width"]);
    }

    [TestMethod]
    public void BorderLeftWidthThickLegal()
    {
        var properties = ParseDeclaration("border-left-width: thick !important");

        Assert.AreEqual("thick", properties["border-left-width"]);
        Assert.AreEqual(4d, CssValueParser.GetActualBorderWidth("thick", null!));
    }

    [TestMethod]
    public void BorderWidthMediumLegal()
    {
        var properties = ParseDeclaration("border-width: medium");

        Assert.AreEqual("medium", properties["border-top-width"]);
        Assert.AreEqual("medium", properties["border-right-width"]);
        Assert.AreEqual("medium", properties["border-bottom-width"]);
        Assert.AreEqual("medium", properties["border-left-width"]);
        Assert.AreEqual(2d, CssValueParser.GetActualBorderWidth("medium", null!));
    }

    [TestMethod]
    public void BorderWidthLengthZeroLegal()
    {
        var properties = ParseDeclaration("border-width: 3px   0");

        Assert.AreEqual("3px", properties["border-top-width"]);
        Assert.AreEqual("3px", properties["border-bottom-width"]);
        Assert.AreEqual("0", properties["border-left-width"]);
        Assert.AreEqual("0", properties["border-right-width"]);
    }

    [TestMethod]
    public void BorderWidthThinLengthLegal()
    {
        var properties = ParseDeclaration("border-width: THIN   1px");

        Assert.AreEqual("thin", properties["border-top-width"]);
        Assert.AreEqual("thin", properties["border-bottom-width"]);
        Assert.AreEqual("1px", properties["border-left-width"]);
        Assert.AreEqual("1px", properties["border-right-width"]);
    }

    [TestMethod]
    public void BorderWidthMediumThinThickLegal()
    {
        var properties = ParseDeclaration("border-width: medium thin thick");

        Assert.AreEqual("medium", properties["border-top-width"]);
        Assert.AreEqual("thin", properties["border-left-width"]);
        Assert.AreEqual("thin", properties["border-right-width"]);
        Assert.AreEqual("thick", properties["border-bottom-width"]);
    }

    [TestMethod]
    public void BorderWidthLengthLengthLengthLengthLegal()
    {
        var properties = ParseDeclaration("border-width:  1px  2px   3px  4px  !important ");

        Assert.AreEqual("1px", properties["border-top-width"]);
        Assert.AreEqual("2px", properties["border-right-width"]);
        Assert.AreEqual("3px", properties["border-bottom-width"]);
        Assert.AreEqual("4px", properties["border-left-width"]);
    }

    [TestMethod]
    public void BorderWidthLengthInEmZeroLegal()
    {
        var properties = ParseDeclaration("border-width:  0.3em 0 ");

        Assert.AreEqual("0.3em", properties["border-top-width"]);
        Assert.AreEqual("0.3em", properties["border-bottom-width"]);
        Assert.AreEqual("0", properties["border-left-width"]);
        Assert.AreEqual("0", properties["border-right-width"]);
    }

    [TestMethod]
    public void BorderWidthMediumZeroLengthThickLegal()
    {
        var properties = ParseDeclaration("border-width:   medium 0 1px thick ");

        Assert.AreEqual("medium", properties["border-top-width"]);
        Assert.AreEqual("0", properties["border-right-width"]);
        Assert.AreEqual("1px", properties["border-bottom-width"]);
        Assert.AreEqual("thick", properties["border-left-width"]);
    }

    [TestMethod]
    public void BorderWidthZerosIllegal()
    {
        // SplitMultiDirectionValues only understands 1-4 values; a 5-value list matches none of its cases,
        // so none of the border-*-width longhands get set at all (a real, silent no-op).
        var properties = ParseDeclaration("border-width: 0 0 0 0 0");

        Assert.IsFalse(properties.ContainsKey("border-top-width"));
        Assert.IsFalse(properties.ContainsKey("border-right-width"));
        Assert.IsFalse(properties.ContainsKey("border-bottom-width"));
        Assert.IsFalse(properties.ContainsKey("border-left-width"));
    }

    // ── border/border-left/border-top/border-right/border-bottom (single-side shorthand) ───────────────────

    [TestMethod]
    [Ignore("not yet spec compliant")]
    public void BorderLeftZeroLegal()
    {
        // PeachPDF: a bare "0" is a legal border width. HTML-Renderer's ParseBorderWidth only recognizes a
        // bare number as a width when it is at least 3 characters long (a number plus a 2-character unit) or
        // matches the "thin"/"medium"/"thick" keywords -- a bare unitless "0" token satisfies neither, so it
        // is not currently recognized as a width at all. Target/intended behavior: width is parsed as "0".
        ParseBorderShorthand("0", out var width, out var style, out var color);

        Assert.AreEqual("0", width);
        Assert.IsNull(style);
        Assert.IsNull(color);
    }

    [TestMethod]
    public void BorderRightLineStyleLegal()
    {
        ParseBorderShorthand("dotted", out var width, out var style, out var color);

        Assert.IsNull(width);
        Assert.AreEqual("dotted", style);
        Assert.IsNull(color);
    }

    [TestMethod]
    public void BorderTopLengthRedLegal()
    {
        ParseBorderShorthand("2px red", out var width, out var style, out var color);

        Assert.AreEqual("2px", width);
        Assert.IsNull(style);
        Assert.AreEqual("red", color);
    }

    [TestMethod]
    [Ignore("not yet spec compliant")]
    public void BorderBottomRgbLegal()
    {
        // PeachPDF: "rgb(255, 100, 0)" is recognized as the border color. HTML-Renderer's ParseBorder
        // tokenizes its value on whitespace only (CommonUtils.GetNextSubString has no notion of parentheses),
        // so a color function containing internal spaces gets split into unrecognizable fragments
        // ("rgb(255,", "100,", "0)") and none of them individually resolve to a width/style/color. Target/
        // intended behavior: the whole function is recognized as the border color.
        ParseBorderShorthand("rgb(255, 100, 0)", out var width, out var style, out var color);

        Assert.IsNull(width);
        Assert.IsNull(style);
        Assert.AreEqual("rgb(255, 100, 0)", color);
    }

    [TestMethod]
    public void BorderGrooveRgbLegal()
    {
        // Same whitespace-tokenizer limitation as BorderBottomRgbLegal means the "rgb(255, 100, 0)" color
        // is not recognized here either -- but the style keyword ("groove"), which is a standalone token,
        // still parses correctly.
        ParseBorderShorthand("GROOVE rgb(255, 100, 0)", out var width, out var style, out var color);

        Assert.AreEqual("groove", style);
        Assert.IsNull(width);
        Assert.IsNull(color);
    }

    [TestMethod]
    public void BorderInsetGreenLengthLegal()
    {
        ParseBorderShorthand("inset  green 3em", out var width, out var style, out var color);

        Assert.AreEqual("3em", width);
        Assert.AreEqual("inset", style);
        Assert.AreEqual("green", color);
    }

    [TestMethod]
    public void BorderRedSolidLengthLegal()
    {
        ParseBorderShorthand("red  SOLID 1px", out var width, out var style, out var color);

        Assert.AreEqual("1px", width);
        Assert.AreEqual("solid", style);
        Assert.AreEqual("red", color);
    }

    [TestMethod]
    public void BorderLengthBlackDoubleLegal()
    {
        ParseBorderShorthand("0.5px black double", out var width, out var style, out var color);

        Assert.AreEqual("0.5px", width);
        Assert.AreEqual("double", style);
        Assert.AreEqual("black", color);
    }

    [TestMethod]
    [Ignore("not yet spec compliant")]
    public void BorderOutSetCurrentColor()
    {
        // PeachPDF: "currentColor" is a legal color keyword. HTML-Renderer has no special handling for
        // "currentColor" -- it is looked up like any other named color (via RAdapter.GetColor), which does
        // not know that name and returns a fully transparent/invalid color, so it is rejected. Target/
        // intended behavior: "currentColor" is recognized as the border color.
        ParseBorderShorthand("1px outset currentColor", out var width, out var style, out var color);

        Assert.AreEqual("1px", width);
        Assert.AreEqual("outset", style);
        Assert.AreEqual("currentColor", color);
    }

    [TestMethod]
    public void BorderOutSetWithNoColor()
    {
        ParseBorderShorthand("1px outset", out var width, out var style, out var color);

        Assert.AreEqual("1px", width);
        Assert.AreEqual("outset", style);
        Assert.IsNull(color);
    }
}
