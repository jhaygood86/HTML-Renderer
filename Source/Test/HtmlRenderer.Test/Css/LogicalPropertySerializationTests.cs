using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/LogicalPropertySerializationTests.cs.
/// PeachPDF resolves a logical box-model shorthand (e.g. <c>margin-block</c>) to its own *logical*
/// longhands (margin-block-start/margin-block-end) at parse time, deferring physical-edge resolution to
/// layout (against the box's own direction/writing-mode). HTML-Renderer's CSS engine port takes a
/// different, simpler path: since this renderer is always LTR / horizontal-tb (confirmed in
/// Source/HtmlRenderer/Core/CssEngine/Factories/PropertyFactory.cs:414-419), each logical property is
/// registered directly against the existing *physical* longhand classes (block-start = top, block-end =
/// bottom, inline-start = left, inline-end = right) - so margin-block expands straight to margin-top/
/// margin-bottom, not to margin-block-start/margin-block-end. The second test below is adapted
/// accordingly; the other two (which only assert about physical properties / the border shorthand) port
/// unchanged.
/// </summary>
[TestClass]
public sealed class LogicalPropertySerializationTests
{
    [TestMethod]
    public void PhysicalLonghands_DoNotSerializeAsLogicalShorthand()
    {
        var sheet = ParseStyleSheet("div { margin-top: 5pt; margin-bottom: 5pt; }");
        var css = sheet.ToCss();

        Assert.IsFalse(css.Contains("margin-block"));
        Assert.IsTrue(css.Contains("margin-top"));
        Assert.IsTrue(css.Contains("margin-bottom"));
    }

    [TestMethod]
    public void LogicalShorthand_ParsesAndExpandsToPhysicalLonghands()
    {
        var sheet = ParseStyleSheet("div { margin-block: 3pt 4pt; }");
        var style = (StyleDeclaration)sheet.Rules.OfType<StyleRule>().Single().Style;

        // HTML-Renderer resolves logical properties to physical classes at registration time (LTR /
        // horizontal-tb only design - PropertyFactory.cs:414-419), so margin-block expands directly to
        // margin-top/margin-bottom rather than to the logical longhands margin-block-start/-end.
        Assert.AreEqual("3pt", style.GetPropertyValue("margin-top"));
        Assert.AreEqual("4pt", style.GetPropertyValue("margin-bottom"));
        Assert.AreEqual(string.Empty, style.GetPropertyValue("margin-block-start"));
        Assert.AreEqual(string.Empty, style.GetPropertyValue("margin-block-end"));
        // And the serialized form uses the physical longhands, never `margin-block`.
        Assert.IsFalse(sheet.ToCss().Contains("margin-block:"));
    }

    [TestMethod]
    public void PhysicalBorderStillReconstructs()
    {
        // The physical `border` shorthand must still reconstruct - the logical exclusion is scoped to the
        // logical shorthands only.
        var sheet = ParseStyleSheet(
            "div { border-top: 1pt solid red; border-right: 1pt solid red; border-bottom: 1pt solid red; border-left: 1pt solid red; }");
        var css = sheet.ToCss();

        Assert.IsTrue(css.Contains("border:"));
    }
}
