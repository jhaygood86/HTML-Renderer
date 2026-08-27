using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Painting;

/// <summary>
/// Verifies <c>text-decoration</c> actually draws a stroke at a plausible baseline-relative position with the
/// right color - <c>CssBox.PaintDecoration</c> is fully implemented for the single-keyword case
/// (underline/line-through/overline), using <see cref="PaintHarness"/>'s recording graphics to assert the real
/// draw-call geometry/color, not just that painting completes.
/// </summary>
/// <remarks>
/// PeachPDF.Tests' source file also covers two cases this fork has no equivalent feature for, so they are
/// intentionally dropped rather than ported:
/// <list type="bullet">
/// <item>Combined decoration lines (<c>text-decoration:underline overline</c> drawing both). HTML-Renderer's
/// <c>CssBoxProperties.TextDecoration</c> is a single string compared with <c>==</c> against one keyword at a
/// time (see <c>CssBox.PaintDecoration</c>) - there is no multi-keyword parsing to draw more than one line.</item>
/// <item><c>text-decoration-color</c> overriding the element color. <c>PaintDecoration</c> always strokes with
/// <c>ActualColor</c> (the text color) - there is no separate decoration-color property anywhere in
/// <c>CssBoxProperties</c>.</item>
/// </list>
/// </remarks>
[DoNotParallelize]
[TestClass]
public sealed class TextDecorationPaintIntegrationTests
{
    [TestMethod]
    public void Underline_DrawsLineBelowRectangleTop()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<span id='s' style='text-decoration:underline; color:rgb(0,0,255)'>text</span>"));
        var s = PaintHarness.FindById(root, "s")!;

        var g = PaintHarness.PaintBox(container, s);

        var line = g.Log.OfType<RecordingGraphics.DrawLineCall>().Single();
        Assert.AreEqual(RColor.FromArgb(0, 0, 255), line.Color);

        var rect = s.Rectangles.Values.Single();
        Assert.IsTrue(line.Y1 > rect.Top, "underline should sit below the rectangle's top edge");
        Assert.IsTrue(line.Y1 <= rect.Bottom, "underline should still sit within the rectangle");
    }

    [TestMethod]
    public void Overline_DrawsLineAtRectangleTop()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<span id='s' style='text-decoration:overline'>text</span>"));
        var s = PaintHarness.FindById(root, "s")!;

        var g = PaintHarness.PaintBox(container, s);

        var line = g.Log.OfType<RecordingGraphics.DrawLineCall>().Single();
        var rect = s.Rectangles.Values.Single();

        Assert.AreEqual(rect.Top, line.Y1, 1);
    }

    [TestMethod]
    public void LineThrough_DrawsLineNearRectangleMiddle()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<span id='s' style='text-decoration:line-through'>text</span>"));
        var s = PaintHarness.FindById(root, "s")!;

        var g = PaintHarness.PaintBox(container, s);

        var line = g.Log.OfType<RecordingGraphics.DrawLineCall>().Single();
        var rect = s.Rectangles.Values.Single();
        var expectedMiddle = rect.Top + rect.Height / 2;

        Assert.AreEqual(expectedMiddle, line.Y1, 1);
    }

    [TestMethod]
    public void Underline_OverlineLineThrough_ProduceDifferentYPositions()
    {
        var underlineY = GetDecorationY("underline");
        var overlineY = GetDecorationY("overline");
        var lineThroughY = GetDecorationY("line-through");

        Assert.IsTrue(overlineY < lineThroughY, "overline should sit above line-through");
        Assert.IsTrue(lineThroughY < underlineY, "line-through should sit above underline");
    }

    [TestMethod]
    public void TextDecorationNone_DrawsNoLine()
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            "<span id='s' style='text-decoration:none'>text</span>"));
        var s = PaintHarness.FindById(root, "s")!;

        var g = PaintHarness.PaintBox(container, s);

        Assert.IsFalse(g.Log.OfType<RecordingGraphics.DrawLineCall>().Any());
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static double GetDecorationY(string decorationLine)
    {
        var (root, container) = PaintHarness.Layout(PaintHarness.Wrap(
            $"<span id='s' style='text-decoration:{decorationLine}'>text</span>"));
        var s = PaintHarness.FindById(root, "s")!;

        var g = PaintHarness.PaintBox(container, s);

        return g.Log.OfType<RecordingGraphics.DrawLineCall>().Single().Y1;
    }
}
