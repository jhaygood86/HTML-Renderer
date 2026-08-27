using System;
using System.Linq;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/CalcEvaluatorTests.cs.
/// Tests for <see cref="CalcEvaluator"/>'s angle-leaf evaluation: a calc() whose value is an
/// &lt;angle&gt; (e.g. <c>calc(1turn * 0.35)</c>) evaluates to radians (the canonical angle unit).
/// This is what lets a conic-gradient stop position be authored as a calc() expression - the
/// Charts.css pie-slice case, whose every stop is <c>calc(1turn * &lt;value&gt;)</c>.
///
/// HTML-Renderer's <see cref="CalcContext"/> constructor takes 4-5 args (hundredPercent, emFactor,
/// remFactor, fontAdjust, returnPoints = false) vs PeachPDF's 3-arg constructor (hundredPercent,
/// emFactor, remFactor). This is a documented non-behavioral API drift, not a real gap: fontAdjust is
/// passed <c>false</c> below (and returnPoints keeps its default of <c>false</c>) to match PeachPDF's
/// 3-arg semantics.
/// </summary>
[TestClass]
public sealed class CalcEvaluatorTests
{
    private static double? EvaluateAngle(string calc)
    {
        var function = CssValueParser.GetCssTokens(calc).OfType<FunctionToken>().Single();
        var node = CalcParser.Parse(function);
        Assert.IsNotNull(node);
        // A full turn is 2π radians; em/rem factors are irrelevant to an angle calc.
        return CalcEvaluator.Evaluate(node!, new CalcContext(2.0 * Math.PI, 0, 0, false));
    }

    [TestMethod]
    [DataRow("calc(1turn * 0.35)", 0.35)]
    [DataRow("calc(1turn * 0.5)", 0.5)]
    [DataRow("calc(0.25turn)", 0.25)]
    public void AngleCalc_TurnScaled_EvaluatesToRadians(string calc, double expectedTurns)
    {
        var radians = EvaluateAngle(calc);
        Assert.IsNotNull(radians);
        Assert.AreEqual(expectedTurns * 2.0 * Math.PI, radians!.Value, 1e-4);
    }

    [TestMethod]
    public void AngleCalc_Degrees_EvaluatesToRadians()
    {
        var radians = EvaluateAngle("calc(90deg + 90deg)");
        Assert.IsNotNull(radians);
        Assert.AreEqual(Math.PI, radians!.Value, 1e-4); // 180deg
    }

    // ── <time>/<resolution> calc leaves (issue #229 gap 2) ───────────────────────
    // These node types are produced by CalcParser for @property `<time>`/`<resolution>` validation.
    // No layout property evaluates or serializes a time/resolution calc, so these tests exercise the
    // CalcEvaluator/CalcSerializer/CalcNode leaf handling that path never reaches directly.

    private static double? Evaluate(string calc)
    {
        var function = CssValueParser.GetCssTokens(calc).OfType<FunctionToken>().Single();
        var node = CalcParser.Parse(function);
        Assert.IsNotNull(node);
        return CalcEvaluator.Evaluate(node!, new CalcContext(0, 0, 0, false));
    }

    [TestMethod]
    [DataRow("calc(1s + 2s)", 3000.0)]    // canonical unit is milliseconds
    [DataRow("calc(500ms)", 500.0)]
    [DataRow("calc(2s * 3)", 6000.0)]
    public void TimeCalc_EvaluatesToMilliseconds(string calc, double expectedMs)
    {
        Assert.AreEqual(expectedMs, Evaluate(calc)!.Value, 1e-3);
    }

    [TestMethod]
    [DataRow("calc(2dppx * 2)", 4.0)]     // canonical unit is dots-per-pixel
    [DataRow("calc(96dpi)", 1.0)]         // 96dpi == 1dppx
    public void ResolutionCalc_EvaluatesToDotsPerPixel(string calc, double expectedDppx)
    {
        Assert.AreEqual(expectedDppx, Evaluate(calc)!.Value, 1e-3);
    }

    private static string Serialize(string calc, CalcCategory category)
    {
        var function = CssValueParser.GetCssTokens(calc).OfType<FunctionToken>().Single();
        var node = CalcParser.Parse(function);
        Assert.IsNotNull(node);
        return CalcSerializer.Serialize(node!, category);
    }

    [TestMethod]
    [DataRow("calc(1s + 2s)", "time", "calc(1s + 2s)")]
    [DataRow("calc(2dppx * 2)", "resolution", "calc(2dppx * 2)")]
    public void TimeResolutionCalc_SerializesToNormalizedText(string calc, string kind, string expected)
    {
        var category = kind == "time" ? CalcCategory.Time : CalcCategory.Resolution;
        Assert.AreEqual(expected, Serialize(calc, category));
    }
}
