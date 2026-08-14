using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/VerticalAlignProperty.cs.
/// PeachPDF.CSS tests only that "vertical-align" keyword/length/percentage values are parsed and stored
/// (via a typed VerticalAlignProperty) -- none of the PeachPDF cases assert an actual visual alignment effect.
/// HTML-Renderer's <see cref="CssBoxProperties.VerticalAlign"/> is a plain string set verbatim by the CSS
/// pipeline, with no legality validation. In <see cref="CssLayoutEngine"/>'s ApplyVerticalAlignment, only
/// "sub" and "super" carry a real layout effect; "top", "bottom", "middle", "text-top" and "text-bottom" are
/// empty-body no-ops for inline layout (table-cell vertical alignment is handled separately by
/// ApplyCellVerticalAlignment and isn't exercised here). Since none of the ported cases assert that visual
/// effect, all keyword cases are ported as plain passing tests of value parsing/storage, read back from a real
/// laid-out <see cref="CssBox"/> via <see cref="LayoutHarness"/>.
/// </summary>
[TestClass]
public sealed class VerticalAlignPropertyTests
{
    private static CssBox GetTargetBox(string value)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap($"<span id='target' style='vertical-align: {value}'>x</span>"));
        var target = LayoutHarness.FindById(root, "target");
        Assert.IsNotNull(target);
        return target;
    }

    [TestMethod]
    [DataRow("baseline")]
    [DataRow("sub")]
    [DataRow("super")]
    [DataRow("top")]
    [DataRow("text-top")]
    [DataRow("middle")]
    [DataRow("bottom")]
    [DataRow("text-bottom")]
    public void VerticalAlignKeywordLegal(string keyword)
    {
        var target = GetTargetBox(keyword);

        Assert.AreEqual(keyword, target.VerticalAlign);
    }

    [TestMethod]
    public void VerticalAlignLengthLegal()
    {
        var target = GetTargetBox("3px");

        Assert.AreEqual("3px", target.VerticalAlign);
    }

    [TestMethod]
    public void VerticalAlignPercentLegal()
    {
        var target = GetTargetBox("25%");

        Assert.AreEqual("25%", target.VerticalAlign);
    }

    [TestMethod]
    public void VerticalAlignNegativeLengthLegal()
    {
        var target = GetTargetBox("-3px");

        Assert.AreEqual("-3px", target.VerticalAlign);
    }

    [TestMethod]
    [Ignore("not yet spec compliant")]
    public void VerticalAlignInvalidKeywordIllegal()
    {
        // PeachPDF: an invalid vertical-align keyword is rejected, so the property reports HasValue == false
        // and the previous (default, "baseline") value is retained. HTML-Renderer's CssBoxProperties.VerticalAlign
        // is a plain string setter with no legality validation, so an invalid keyword is currently accepted and
        // stored verbatim instead of being rejected. Target/intended behavior: the box keeps its default
        // "baseline" alignment when given an invalid keyword.
        var target = GetTargetBox("wavy");

        Assert.AreEqual("baseline", target.VerticalAlign);
    }
}
