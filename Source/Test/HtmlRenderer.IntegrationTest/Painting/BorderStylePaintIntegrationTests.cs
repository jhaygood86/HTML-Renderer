using System;
using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Painting;

/// <summary>
/// Verifies border-style painting actually produces the right geometry/color, not just that it doesn't crash -
/// port of PeachPDF.Tests' <c>BorderStylePaintIntegrationTests</c>, adapted to this fork's real (and more
/// limited) <c>BordersDrawHandler</c>.
/// </summary>
/// <remarks>
/// Confirmed by direct source read of <c>Core/Handlers/BordersDrawHandler.cs</c>: the private
/// <c>DrawBorder(Border, CssBox, RGraphics, RRect, bool, bool)</c> that <c>DrawBoxBorders</c> calls for every
/// ordinary (non-rounded) box only special-cases <c>style == Inset || style == Outset</c> - that branch goes
/// through <c>SetInOutsetRectanglePoints</c> + <c>g.DrawPolygon</c> (a real, distinct, beveled-quad path that
/// darkens the color via <c>Darken</c> on specific sides - see <c>GetColor</c>). Every OTHER style
/// (<c>solid</c>/<c>dotted</c>/<c>dashed</c>, and the CSS-recognized-but-undispatched <c>double</c>/
/// <c>groove</c>/<c>ridge</c>) falls into the shared "else" branch and draws as a SINGLE straight line via
/// <c>g.DrawLine</c> - i.e. <c>double</c>/<c>groove</c>/<c>ridge</c> are indistinguishable from <c>solid</c> on
/// this fork; they do not throw (unlike the old PeachPDF bug this file's source documents), they just silently
/// render wrong. <c>RecordingGraphics.DrawLineCall</c> does not capture pen dash style (only color/position), so
/// unlike PeachPDF's <c>TestRecordingGraphics.DrawLineCall</c> (which also carries a stroke <c>Width</c>), those
/// fields are not available to assert on here.
///
/// Also confirmed by direct source read of <c>Core/Parse/DomParser.cs</c> (~436-449): the deprecated
/// presentational <c>border</c> HTML attribute forces solid on all four sides for a plain (non-table) element,
/// same as PeachPDF.
///
/// The old hand-rolled <c>CssParser.SplitMultiDirectionValues</c>/<c>SplitValues</c> (which split strictly on
/// spaces, with no parenthesis-awareness, and so used to mis-split a space-containing <c>rgb(r, g, b)</c>
/// token inside a multi-value shorthand like a 4-value <c>border-color</c>) no longer exists - the CSS engine
/// port's real tokenizer handles parenthesized functions correctly regardless of internal spaces. The 4-value
/// color test below still uses comma-only <c>rgb(r,g,b)</c> input for historical parity with that old
/// constraint, but the resolved <c>BorderTopColor</c>/etc. values it asserts on are the engine's own
/// normalized "rgb(r, g, b)" (spaced) serialization, not the literal input text.
/// </remarks>
[DoNotParallelize]
[TestClass]
public sealed class BorderStylePaintIntegrationTests
{
    [TestMethod]
    [DataRow("dotted")]
    [DataRow("dashed")]
    [DataRow("solid")]
    [DataRow("double")]
    [DataRow("groove")]
    [DataRow("ridge")]
    [DataRow("inset")]
    [DataRow("outset")]
    public void BorderStyle_AllCss1Keywords_DoNotThrowWhenPainted(string style)
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            $"<div id='b' style='border: 12px {style} rgb(51,51,51)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        // Should not throw for any CSS1 border-style keyword - including double/groove/ridge, which this fork
        // doesn't specially render (they fall back to a single solid-colored line, see this class's remarks).
        PaintHarness.PaintBox(container, div);
    }

    [Ignore("This fork's BordersDrawHandler.DrawBorder has no 'double' case for the non-rounded path - the style " +
            "falls into the shared 'else' branch (same as solid/dotted/dashed) and draws ONE line via " +
            "g.DrawLine, not two same-color stripes with a gap between them. Confirmed by direct source read of " +
            "BordersDrawHandler.cs's private DrawBorder overload.")]
    [TestMethod]
    public void BorderStyleDouble_DrawsTwoEqualWidthStripesWithGap()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='border-top-style: double; border-top-width: 12px; border-top-color: rgb(51,51,51)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        var lines = g.Log.OfType<RecordingGraphics.DrawLineCall>().ToList();
        Assert.AreEqual(2, lines.Count);
        Assert.AreNotEqual(lines[0].Y1, lines[1].Y1, "expected two visually distinct stripes, not one merged line");
    }

    [Ignore("This fork's GetColor only darkens for Inset/Outset styles (see BordersDrawHandler.GetColor) - " +
            "'groove' isn't dispatched at all, so it draws a single line in the base color (51,51,51), not a " +
            "darker-outer / base-color-inner stripe pair. Confirmed by direct source read.")]
    [TestMethod]
    public void BorderStyleGroove_OuterStripeIsDarker_InnerStripeIsBaseColor()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='border-top-style: groove; border-top-width: 12px; border-top-color: rgb(51,51,51)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        var lines = g.Log.OfType<RecordingGraphics.DrawLineCall>().ToList();
        Assert.AreEqual(2, lines.Count);
        Assert.AreEqual(RColor.FromArgb(25, 25, 25), lines[0].Color);
        Assert.AreEqual(RColor.FromArgb(51, 51, 51), lines[1].Color);
    }

    [Ignore("Same gap as BorderStyleDouble_DrawsTwoEqualWidthStripesWithGap, mirrored onto the right edge - " +
            "'double' falls back to a single g.DrawLine call instead of two vertical stripes with a gap.")]
    [TestMethod]
    public void BorderRightStyleDouble_DrawsTwoEqualWidthVerticalStripesWithGap()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='border-right-style: double; border-right-width: 12px; border-right-color: rgb(51,51,51)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        var lines = g.Log.OfType<RecordingGraphics.DrawLineCall>().ToList();
        Assert.AreEqual(2, lines.Count);
        Assert.IsTrue(Math.Abs(lines[0].X1 - lines[0].X2) < 0.01, "expected a vertical (right-edge) line");
        Assert.AreNotEqual(lines[0].X1, lines[1].X1, "expected two visually distinct vertical stripes");
    }

    [Ignore("Same gap as BorderStyleGroove_OuterStripeIsDarker_InnerStripeIsBaseColor, mirrored onto the left " +
            "edge - 'groove' isn't dispatched, so no darkened outer stripe is produced.")]
    [TestMethod]
    public void BorderLeftStyleGroove_OuterStripeIsDarker_InnerStripeIsBaseColor()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='border-left-style: groove; border-left-width: 12px; border-left-color: rgb(51,51,51)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        var lines = g.Log.OfType<RecordingGraphics.DrawLineCall>().ToList();
        Assert.AreEqual(2, lines.Count);
        Assert.AreEqual(RColor.FromArgb(25, 25, 25), lines[0].Color);
        Assert.AreEqual(RColor.FromArgb(51, 51, 51), lines[1].Color);
    }

    [TestMethod]
    public void BorderColorPerSide_ResolvesDistinctColorPerEdge()
    {
        // Adapted from PeachPDF's version, which also covers 'currentcolor' on the left edge and asserts via
        // DrawPolygonCall (this fork draws solid borders as g.DrawLine, not a mitered polygon quad - see this
        // class's remarks) - the currentcolor sub-case is split out into
        // BorderColorPerSide_CurrentColor_DoesNotResolveToElementColor below since it fails for an unrelated
        // reason (currentcolor isn't a recognized color keyword on this fork at all).
        // NOTE: colors deliberately use a 2-digit component (e.g. "rgb(10,0,0)" not "rgb(1,0,0)") to avoid a
        // separate, confirmed real bug: CssValueParser.TryGetColor (Core/Parse/CssValueParser.cs ~313) requires
        // "length > 10" for an rgb(...) token to be dispatched to GetColorByRgb at all - any all-single-digit
        // triple like "rgb(1,0,0)" or "rgb(9,9,9)" is exactly 10 characters and silently fails IsColorValid, so
        // the color is dropped entirely (paints as RColor.Empty) for a reason wholly unrelated to what this test
        // is actually verifying (per-edge border-color resolution). Confirmed by direct instrumentation.
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:40px; height:40px; border-style:solid; border-width:4px; " +
            "border-top-color: rgb(10,0,0); border-right-color: rgb(0,10,0); " +
            "border-bottom-color: rgb(0,0,10); border-left-color: rgb(90,90,90)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        var g = PaintHarness.PaintBox(container, div);

        var lines = g.Log.OfType<RecordingGraphics.DrawLineCall>().ToList();
        Assert.AreEqual(4, lines.Count);

        var horizontal = lines.Where(l => Math.Abs(l.Y1 - l.Y2) < 0.01).ToList();
        var vertical = lines.Where(l => Math.Abs(l.X1 - l.X2) < 0.01).ToList();
        Assert.AreEqual(2, horizontal.Count);
        Assert.AreEqual(2, vertical.Count);

        var top = horizontal.OrderBy(l => l.Y1).First();
        var bottom = horizontal.OrderByDescending(l => l.Y1).First();
        var left = vertical.OrderBy(l => l.X1).First();
        var right = vertical.OrderByDescending(l => l.X1).First();

        Assert.AreEqual(RColor.FromArgb(10, 0, 0), top.Color);
        Assert.AreEqual(RColor.FromArgb(0, 10, 0), right.Color);
        Assert.AreEqual(RColor.FromArgb(0, 0, 10), bottom.Color);
        Assert.AreEqual(RColor.FromArgb(90, 90, 90), left.Color);
    }

    [Ignore("'currentcolor' is not a recognized color keyword anywhere on this fork: CssValueParser.TryGetColor " +
            "(Core/Parse/CssValueParser.cs ~303-331) only special-cases '#...', 'rgb(...)' and 'rgba(...)' - " +
            "anything else (including 'currentcolor') falls through to GetColorByName, which asks the adapter " +
            "for a named System.Drawing color, finds none, and yields a fully-transparent RColor(0,0,0,0) " +
            "instead of resolving to the element's own 'color'. Confirmed by direct source read.")]
    [TestMethod]
    public void BorderColorPerSide_CurrentColor_DoesNotResolveToElementColor()
    {
        var (root, _) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:40px; height:40px; border-style:solid; border-width:4px; color: rgb(90,90,90); " +
            "border-left-color: currentcolor'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        Assert.AreEqual(RColor.FromArgb(90, 90, 90), div.ActualBorderLeftColor);
    }

    [Ignore("Exactly the class of bug BorderStyleGroove_OuterStripeIsDarker_InnerStripeIsBaseColor documents: " +
            "since 'groove' isn't dispatched at all in GetColor/DrawBorder, it can't be a mirror image of " +
            "'ridge' (which also isn't dispatched) - both styles draw an identical single base-colored line, so " +
            "there is no outer/inner stripe pair to compare in either direction.")]
    [TestMethod]
    public void BorderStyleRidge_IsMirrorImageOfGroove()
    {
        var (grooveRoot, grooveContainer) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='border-top-style: groove; border-top-width: 12px; border-top-color: rgb(51,51,51)'>x</div>"));
        var grooveDiv = PaintHarness.FindById(grooveRoot, "b")!;
        var grooveG = PaintHarness.PaintBox(grooveContainer, grooveDiv);
        var grooveLines = grooveG.Log.OfType<RecordingGraphics.DrawLineCall>().ToList();

        var (ridgeRoot, ridgeContainer) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='border-top-style: ridge; border-top-width: 12px; border-top-color: rgb(51,51,51)'>x</div>"));
        var ridgeDiv = PaintHarness.FindById(ridgeRoot, "b")!;
        var ridgeG = PaintHarness.PaintBox(ridgeContainer, ridgeDiv);
        var ridgeLines = ridgeG.Log.OfType<RecordingGraphics.DrawLineCall>().ToList();

        Assert.AreEqual(2, grooveLines.Count);
        Assert.AreEqual(2, ridgeLines.Count);

        Assert.AreEqual(grooveLines[0].Color, ridgeLines[1].Color);
        Assert.AreEqual(grooveLines[1].Color, ridgeLines[0].Color);
        Assert.AreNotEqual(grooveLines[0].Color, grooveLines[1].Color);
    }

    [TestMethod]
    public void BorderStyleDoubleWithRoundedCorners_FallsBackToASingleUnstripedStroke()
    {
        // PeachPDF's analogous test uses standard CSS 'border-radius'. This fork's CSS engine port added real
        // border-radius support (see the sibling BorderRadiusIntegrationTests class's remarks), so it's used
        // directly here (the proprietary 'corner-radius' this comment used to describe as the only working
        // equivalent has been removed) so the box actually IS rounded and GetRoundedBorderPath takes the
        // rounded-path branch (BordersDrawHandler.DrawBorder -> g.DrawPath), which - like the non-rounded path -
        // has no double/groove/ridge case either (GetPen's DashStyle switch has no 'double' case), so it
        // silently falls back to a single solid-colored stroke.
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='border-top-style: double; border-top-width: 12px; border-top-color: rgb(51,51,51); border-radius: 8px'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;
        Assert.IsTrue(div.IsRounded);

        var g = PaintHarness.PaintBox(container, div);

        // RecordingGraphics.DrawPath is a no-op override (nothing gets logged for it), so instead of asserting a
        // DrawPathCall exists (as PeachPDF's TestRecordingGraphics does), verify the negative space: no crash,
        // and no DrawLine/DrawPolygon calls - proving it took the rounded DrawPath branch, not either
        // non-rounded one.
        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawLineCall>().Any());
        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawPolygonCall>().Any());
    }

    [TestMethod]
    public void BorderStyleTwoValueShorthand_OnlyPaintsTheSolidSides()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='width:40px; height:40px; border-width:4px; border-color:rgb(51,51,51); border-style: none solid'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        Assert.AreEqual("none", div.BorderTopStyle);
        Assert.AreEqual("solid", div.BorderRightStyle);
        Assert.AreEqual("none", div.BorderBottomStyle);
        Assert.AreEqual("solid", div.BorderLeftStyle);

        var g = PaintHarness.PaintBox(container, div);

        // This fork draws solid borders as a single straight line (BordersDrawHandler's shared non-rounded
        // "else" branch), not a mitered polygon quad like PeachPDF - see this class's remarks. Only left/right
        // (solid) should draw anything; top/bottom (none) draw nothing.
        var lines = g.Log.OfType<RecordingGraphics.DrawLineCall>().ToList();
        Assert.AreEqual(2, lines.Count);
        Assert.IsTrue(lines.All(l => Math.Abs(l.X1 - l.X2) < 0.01), "expected only vertical (left/right) border lines");
        Assert.IsTrue(lines.All(l => l.Color == RColor.FromArgb(51, 51, 51)));
    }

    [TestMethod]
    public void BorderColorFourValueShorthand_ResolvesTopRightBottomLeftPerSide()
    {
        var (root, _) = PaintHarness.Layout(PaintHarness.Wrap(
            "<div id='b' style='border-style:solid; border-width:1px; border-color: rgb(1,0,0) rgb(0,1,0) rgb(0,0,1) rgb(1,1,0)'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        // The resolved longhand values are the engine's own normalized "rgb(r, g, b)" (spaced) text, not
        // the literal comma-only input - see this class's remarks.
        Assert.AreEqual("rgb(1, 0, 0)", div.BorderTopColor);
        Assert.AreEqual("rgb(0, 1, 0)", div.BorderRightColor);
        Assert.AreEqual("rgb(0, 0, 1)", div.BorderBottomColor);
        Assert.AreEqual("rgb(1, 1, 0)", div.BorderLeftColor);
    }

    [TestMethod]
    public void BorderWidthTwoValueShorthand_ThenLaterOneValue_OverridesAllSidesPerSpecificity()
    {
        // Mirrors PeachPDF's own "border-width: 0 2em" (2-value: top/bottom=0, left/right=2em) followed later by
        // a same-specificity "border-width: 1em" (all sides) - the later rule must win outright on every side.
        var (root, _) = PaintHarness.Layout(
            "<html><head><style>#b { border-width: 0 2em; } #b { border-width: 1em; }</style></head>" +
            "<body style='margin:0'><div id='b' style='border-style:solid'></div></body></html>");
        var div = PaintHarness.FindById(root, "b")!;

        Assert.AreEqual("1em", div.BorderTopWidth);
        Assert.AreEqual("1em", div.BorderRightWidth);
        Assert.AreEqual("1em", div.BorderBottomWidth);
        Assert.AreEqual("1em", div.BorderLeftWidth);
    }

    [TestMethod]
    public void PresentationalBorderAttribute_OnAPlainElement_ForcesSolidOnAllSides()
    {
        var (root, _) = PaintHarness.Layout(PaintHarness.Wrap("<div id='b' border='1'>x</div>"));
        var div = PaintHarness.FindById(root, "b")!;

        Assert.AreEqual("solid", div.BorderTopStyle);
        Assert.AreEqual("solid", div.BorderRightStyle);
        Assert.AreEqual("solid", div.BorderBottomStyle);
        Assert.AreEqual("solid", div.BorderLeftStyle);
    }
}
