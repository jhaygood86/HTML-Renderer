using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Painting;

/// <summary>
/// Standard CSS <c>border-radius</c> (shorthand and all four longhands) is not recognized anywhere in this
/// fork's Core (confirmed: zero hits for "border-radius"/"border-top-left-radius"/etc. in
/// <c>Utils/CssUtils.cs</c>'s property dispatch, the only place inline/stylesheet CSS property names are mapped
/// onto <c>CssBox</c> members) - it is silently dropped as an unrecognized declaration, so corners stay square
/// no matter what value is given. Instead this fork has a proprietary equivalent: the <c>corner-radius</c>
/// shorthand and <c>corner-nw-radius</c>/<c>corner-ne-radius</c>/<c>corner-se-radius</c>/<c>corner-sw-radius</c>
/// longhands (<c>Utils/CssUtils.cs</c> ~99-107/258-270, backed by <c>CssBoxProperties.CornerRadius</c>/
/// <c>CornerNwRadius</c>/etc., ~Dom/CssBoxProperties.cs 277-337), with computed <c>ActualCornerNw</c>/
/// <c>ActualCornerNe</c>/<c>ActualCornerSe</c>/<c>ActualCornerSw</c> doubles and a boolean <c>IsRounded</c>
/// (true if any corner is greater than 0) - see ~1110-1172 of the same file.
/// </summary>
/// <remarks>
/// PeachPDF's source file also covers elliptical radii (distinct X/Y per corner via <c>border-radius: 40pt /
/// 15pt</c>), percentage-relative radii, and radius-overlap reduction via a <c>ComputeRadii</c> method. None of
/// those have ANY representable equivalent on this fork: the proprietary <c>corner-*-radius</c> properties are
/// single values (no X/Y distinction at all - see the four single-value properties above), radius resolution
/// hardcodes its percentage basis to 0 (<c>CssValueParser.ParseLength(CornerNwRadius, 0, this)</c>, ~line 1116 -
/// so a percentage corner radius always resolves to 0 regardless of box size), and no <c>ComputeRadii</c> or
/// overlap-reduction method exists anywhere in Core. Porting those cases would require asserting on properties
/// that don't exist, so they are intentionally dropped rather than forced to compile under a false premise -
/// only the circular-radius PeachPDF cases are ported below (mapped onto this fork's corner-name properties:
/// north-west/north-east/south-east/south-west correspond to top-left/top-right/bottom-right/bottom-left).
/// </remarks>
[DoNotParallelize]
[TestClass]
public sealed class BorderRadiusIntegrationTests
{
    private const double Delta = 0.5;

    [Ignore("border-radius (and every longhand) is not a recognized CSS property anywhere in this fork's Core - " +
            "see this class's remarks. It is silently dropped, so all four corners stay at their default 0, and " +
            "IsRounded stays false instead of true.")]
    [TestMethod]
    public void BorderRadius_Shorthand_SetsAllCorners()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='c' style='width:200px;height:100px;border-radius:10px;border:1px solid black'></div>"));
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(10.0, box.ActualCornerNw, Delta);
        Assert.AreEqual(10.0, box.ActualCornerNe, Delta);
        Assert.AreEqual(10.0, box.ActualCornerSe, Delta);
        Assert.AreEqual(10.0, box.ActualCornerSw, Delta);
        Assert.IsTrue(box.IsRounded);
    }

    [Ignore("border-radius is not a recognized CSS property on this fork - see this class's remarks. All four " +
            "corners stay at 0 instead of the opposing-pair values (top-left/bottom-right=10, " +
            "top-right/bottom-left=20) the shorthand should assign.")]
    [TestMethod]
    public void BorderRadius_TwoValues_SetsOpposingCorners()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='c' style='width:200px;height:100px;border-radius:10px 20px'></div>"));
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(10.0, box.ActualCornerNw, Delta); // top-left
        Assert.AreEqual(20.0, box.ActualCornerNe, Delta); // top-right
        Assert.AreEqual(10.0, box.ActualCornerSe, Delta); // bottom-right
        Assert.AreEqual(20.0, box.ActualCornerSw, Delta); // bottom-left
    }

    [Ignore("border-radius is not a recognized CSS property on this fork - see this class's remarks. All four " +
            "corners stay at 0 instead of the four individually-assigned values.")]
    [TestMethod]
    public void BorderRadius_FourValues_SetsAllCornersIndividually()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='c' style='width:200px;height:100px;border-radius:5px 10px 15px 20px'></div>"));
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(5.0, box.ActualCornerNw, Delta);  // top-left
        Assert.AreEqual(10.0, box.ActualCornerNe, Delta); // top-right
        Assert.AreEqual(15.0, box.ActualCornerSe, Delta); // bottom-right
        Assert.AreEqual(20.0, box.ActualCornerSw, Delta); // bottom-left
    }

    [Ignore("border-top-left-radius (the standard longhand) is not a recognized CSS property on this fork either " +
            "- see this class's remarks. box.ActualCornerNw stays 0 and IsRounded stays false instead of true.")]
    [TestMethod]
    public void BorderTopLeftRadius_Longhand_SetsOnlyTopLeft()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='c' style='width:200px;height:100px;border-top-left-radius:12px'></div>"));
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(12.0, box.ActualCornerNw, Delta);
        Assert.AreEqual(0.0, box.ActualCornerNe, Delta);
        Assert.AreEqual(0.0, box.ActualCornerSe, Delta);
        Assert.AreEqual(0.0, box.ActualCornerSw, Delta);
        Assert.IsTrue(box.IsRounded);
    }

    [TestMethod]
    public void BorderRadius_Zero_IsNotRounded()
    {
        // No radius declared at all - the default-square-corners baseline is identical on both engines, so this
        // one genuinely passes here; it isn't exercising the border-radius-unrecognized gap at all.
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='c' style='width:200px;height:100px'></div>"));
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.IsFalse(box.IsRounded);
    }

    // ─── Supplementary: this fork's proprietary corner-radius/corner-*-radius syntax (not ported from PeachPDF -
    // demonstrates the equivalent feature this fork actually has working, per this class's remarks) ────────────

    [TestMethod]
    public void CornerRadius_Shorthand_SingleValue_SetsAllCornersAndIsRounded()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='c' style='width:200px;height:100px;corner-radius:10px'></div>"));
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(10.0, box.ActualCornerNw, Delta);
        Assert.AreEqual(10.0, box.ActualCornerNe, Delta);
        Assert.AreEqual(10.0, box.ActualCornerSe, Delta);
        Assert.AreEqual(10.0, box.ActualCornerSw, Delta);
        Assert.IsTrue(box.IsRounded);
    }

    [TestMethod]
    public void CornerNwRadius_Longhand_SetsOnlyTopLeftCorner()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='c' style='width:200px;height:100px;corner-nw-radius:12px'></div>"));
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(12.0, box.ActualCornerNw, Delta);
        Assert.AreEqual(0.0, box.ActualCornerNe, Delta);
        Assert.AreEqual(0.0, box.ActualCornerSe, Delta);
        Assert.AreEqual(0.0, box.ActualCornerSw, Delta);
        Assert.IsTrue(box.IsRounded);
    }

    [TestMethod]
    public void CornerRadius_FourValues_AssignsCornersInProprietaryNeNwSeSwOrder()
    {
        // Unlike standard CSS border-radius (top-left/top-right/bottom-right/bottom-left order), this fork's
        // 4-value corner-radius shorthand assigns in NE/NW/SE/SW order - confirmed directly from
        // CssBoxProperties.CornerRadius's setter (Dom/CssBoxProperties.cs ~303-308):
        //   case 4: CornerNeRadius = r[0]; CornerNwRadius = r[1]; CornerSeRadius = r[2]; CornerSwRadius = r[3];
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='c' style='width:200px;height:100px;corner-radius:5px 10px 15px 20px'></div>"));
        var box = LayoutHarness.FindById(root, "c")!;

        Assert.AreEqual(5.0, box.ActualCornerNe, Delta);
        Assert.AreEqual(10.0, box.ActualCornerNw, Delta);
        Assert.AreEqual(15.0, box.ActualCornerSe, Delta);
        Assert.AreEqual(20.0, box.ActualCornerSw, Delta);
        Assert.IsTrue(box.IsRounded);
    }
}
