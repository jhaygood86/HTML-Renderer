using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Painting;

/// <summary>
/// Ported from PeachPDF.Tests/Integration/BorderRadiusIntegrationTests.cs.
/// Standard CSS <c>border-radius</c> (shorthand and all four longhands) used to be entirely unrecognized
/// by this fork's Core, which instead had a proprietary <c>corner-radius</c>/<c>corner-{nw,ne,se,sw}-radius</c>
/// mechanism (backed by <c>CssBoxProperties.CornerRadius</c>/<c>ActualCornerNw</c>/etc.) as its only way to
/// get rounded corners. The CSS engine port replaced that proprietary mechanism entirely with a real,
/// spec-compliant implementation (see Source/HtmlRenderer/Core/Dom/CssBoxProperties.cs ~1257-1400):
///   - the standard 4-longhand model (<c>BorderTopLeftRadius</c>/<c>BorderTopRightRadius</c>/
///     <c>BorderBottomRightRadius</c>/<c>BorderBottomLeftRadius</c>), each carrying independent X/Y radii
///     (elliptical corners, via a "h v" pair per longhand, or "/" in the shorthand);
///   - the <c>border-radius</c> shorthand's real CSS Backgrounds 3 5.5 1/2/3/4-value expansion, in the
///     correct top-left/top-right/bottom-right/bottom-left order (unlike the old proprietary shorthand's
///     NE/NW/SE/SW order);
///   - percentage-of-box-dimension resolution (<c>ActualBorderTopLeftRadiusX</c> resolves a percentage
///     against <c>Size.Width</c>, <c>...RadiusY</c> against <c>Size.Height</c> - not hardcoded to 0); and
///   - overlap reduction via <c>ComputeRadii</c>, exactly CSS Backgrounds 3 4's "scale every radius down
///     proportionally if adjacent corners would sum to more than an edge's length" algorithm.
/// So every case below - including the four that were previously <c>[Ignore]</c>d because border-radius
/// didn't exist at all - now genuinely passes against the real properties
/// (<c>ActualBorderTopLeftRadiusX/Y</c> etc., <c>IsRounded</c>, <c>ComputeRadii</c>), and this file also
/// ports the elliptical/percentage/overlap-reduction cases from PeachPDF's original test file that the
/// old proprietary mechanism had no representable equivalent for at all. The proprietary
/// <c>corner-radius</c>/<c>corner-*-radius</c> syntax and its backing <c>ActualCornerNw/Ne/Se/Sw</c>/
/// <c>CornerRadius</c> members have been removed from Core entirely, so the "supplementary" tests this
/// file used to carry for them are gone too.
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class BorderRadiusIntegrationTests
{
    private const double Delta = 0.5;

    // ── Circular radii (symmetric X = Y) ────────────────────────────────────────────────────────────────

    [TestMethod]
    public void BorderRadius_Shorthand_SetsAllCorners()
    {
        var box = FindDivBox("border-radius:10px;border:1px solid black");

        Assert.AreEqual(10.0, box.ActualBorderTopLeftRadiusX, Delta);
        Assert.AreEqual(10.0, box.ActualBorderTopLeftRadiusY, Delta);
        Assert.AreEqual(10.0, box.ActualBorderTopRightRadiusX, Delta);
        Assert.AreEqual(10.0, box.ActualBorderTopRightRadiusY, Delta);
        Assert.AreEqual(10.0, box.ActualBorderBottomRightRadiusX, Delta);
        Assert.AreEqual(10.0, box.ActualBorderBottomRightRadiusY, Delta);
        Assert.AreEqual(10.0, box.ActualBorderBottomLeftRadiusX, Delta);
        Assert.AreEqual(10.0, box.ActualBorderBottomLeftRadiusY, Delta);
        Assert.IsTrue(box.IsRounded);
    }

    [TestMethod]
    public void BorderRadius_TwoValues_SetsOpposingCorners()
    {
        var box = FindDivBox("border-radius:10px 20px");

        Assert.AreEqual(10.0, box.ActualBorderTopLeftRadiusX, Delta);      // top-left
        Assert.AreEqual(20.0, box.ActualBorderTopRightRadiusX, Delta);     // top-right
        Assert.AreEqual(10.0, box.ActualBorderBottomRightRadiusX, Delta);  // bottom-right
        Assert.AreEqual(20.0, box.ActualBorderBottomLeftRadiusX, Delta);   // bottom-left
    }

    [TestMethod]
    public void BorderRadius_FourValues_SetsAllCornersIndividually()
    {
        var box = FindDivBox("border-radius:5px 10px 15px 20px");

        Assert.AreEqual(5.0, box.ActualBorderTopLeftRadiusX, Delta);       // top-left
        Assert.AreEqual(10.0, box.ActualBorderTopRightRadiusX, Delta);     // top-right
        Assert.AreEqual(15.0, box.ActualBorderBottomRightRadiusX, Delta);  // bottom-right
        Assert.AreEqual(20.0, box.ActualBorderBottomLeftRadiusX, Delta);   // bottom-left
    }

    [TestMethod]
    public void BorderTopLeftRadius_Longhand_SetsOnlyTopLeft()
    {
        var box = FindDivBox("border-top-left-radius:12px");

        Assert.AreEqual(12.0, box.ActualBorderTopLeftRadiusX, Delta);
        Assert.AreEqual(12.0, box.ActualBorderTopLeftRadiusY, Delta);
        Assert.AreEqual(0.0, box.ActualBorderTopRightRadiusX, Delta);
        Assert.AreEqual(0.0, box.ActualBorderBottomRightRadiusX, Delta);
        Assert.AreEqual(0.0, box.ActualBorderBottomLeftRadiusX, Delta);
        Assert.IsTrue(box.IsRounded);
    }

    [TestMethod]
    public void BorderRadius_Zero_IsNotRounded()
    {
        var box = FindDivBox("");
        Assert.IsFalse(box.IsRounded);
    }

    // ── Elliptical radii (X != Y) ───────────────────────────────────────────────────────────────────────

    [TestMethod]
    public void BorderRadius_EllipticalShorthand_SetsAllCornersXAndY()
    {
        // border-radius: 40px / 15px -> each corner: X=40, Y=15
        var box = FindDivBox("border-radius:40px / 15px");

        Assert.AreEqual(40.0, box.ActualBorderTopLeftRadiusX, Delta);
        Assert.AreEqual(15.0, box.ActualBorderTopLeftRadiusY, Delta);
        Assert.AreEqual(40.0, box.ActualBorderTopRightRadiusX, Delta);
        Assert.AreEqual(15.0, box.ActualBorderTopRightRadiusY, Delta);
        Assert.AreEqual(40.0, box.ActualBorderBottomRightRadiusX, Delta);
        Assert.AreEqual(15.0, box.ActualBorderBottomRightRadiusY, Delta);
        Assert.AreEqual(40.0, box.ActualBorderBottomLeftRadiusX, Delta);
        Assert.AreEqual(15.0, box.ActualBorderBottomLeftRadiusY, Delta);
    }

    [TestMethod]
    public void BorderTopLeftRadius_Longhand_EllipticalValues()
    {
        // border-top-left-radius: 15px 25px -> X=15, Y=25
        var box = FindDivBox("border-top-left-radius:15px 25px");

        Assert.AreEqual(15.0, box.ActualBorderTopLeftRadiusX, Delta);
        Assert.AreEqual(25.0, box.ActualBorderTopLeftRadiusY, Delta);
        Assert.AreEqual(0.0, box.ActualBorderTopRightRadiusX, Delta);
    }

    // ── Percentage values ────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    public void BorderRadius_Percentage_ResolvesRelativeToDimensions()
    {
        // 200x100 box; border-radius: 50% -> X = 50% of 200 = 100, Y = 50% of 100 = 50
        var box = FindDivBox("border-radius:50%");

        Assert.AreEqual(100.0, box.ActualBorderTopLeftRadiusX, 1);
        Assert.AreEqual(50.0, box.ActualBorderTopLeftRadiusY, 1);
    }

    // ── Overlapping radii reduction ─────────────────────────────────────────────────────────────────────

    [TestMethod]
    public void BorderRadius_OverlappingRadii_AreReducedProportionally()
    {
        // 100x100 box; border-radius: 60px - adjacent radii sum to 120 > 100, so all must scale by
        // 100/120 (roughly 0.833), giving ~50px at the boundary.
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='c' style='width:100px;height:100px;border-radius:60px'></div>"));
        var box = LayoutHarness.FindById(root, "c")!;

        var radii = box.ComputeRadii(new RRect(0, 0, 100, 100));

        Assert.AreEqual(100.0, radii.TLX + radii.TRX, 2);
        Assert.AreEqual(100.0, radii.BLX + radii.BRX, 2);
        Assert.AreEqual(100.0, radii.TLY + radii.BLY, 2);
        Assert.AreEqual(100.0, radii.TRY + radii.BRY, 2);
    }

    [TestMethod]
    public void BorderRadius_NonOverlappingRadii_AreNotChanged()
    {
        // 200x200 box; border-radius: 30px - 30+30=60 < 200, no reduction.
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='c' style='width:200px;height:200px;border-radius:30px'></div>"));
        var box = LayoutHarness.FindById(root, "c")!;

        var radii = box.ComputeRadii(new RRect(0, 0, 200, 200));

        Assert.AreEqual(30.0, radii.TLX, 2);
        Assert.AreEqual(30.0, radii.TLY, 2);
    }

    private static CssBox FindDivBox(string css)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            $"<div id='c' style='width:200px;height:100px;{css}'></div>"));
        return LayoutHarness.FindById(root, "c")!;
    }
}
