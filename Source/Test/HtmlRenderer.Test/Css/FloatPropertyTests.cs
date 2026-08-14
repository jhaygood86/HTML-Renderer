using HtmlRenderer.Test.TestSupport;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/FloatClearProperty.cs.
/// Only the `float` cases apply: HTML-Renderer has no `clear` CSS property at all (no ClearProperty
/// type and no CssBoxProperties.Clear field/property - confirmed by grepping "Clear" across
/// CssBoxProperties.cs and HtmlConstants.cs), so every `clear` case from the source file was dropped.
/// The "invalid keyword" float case was also dropped: TheArtOfDev.HtmlRenderer.Core.Parse.CssParser
/// does not validate `float` values against a keyword set, and CssUtils.SetPropertyValue assigns
/// whatever string was parsed straight to CssBox.Float with no rejection path, so there is no "illegal
/// keyword" outcome to observe in this fork.
/// Exercised via the real box tree (LayoutHarness + inline style) rather than raw property parsing, so
/// the assertion is against the actual CssBoxProperties.Float value a laid-out box ends up with.
/// </summary>
[TestClass]
public sealed class FloatPropertyTests
{
    [TestMethod]
    [DataRow("left")]
    [DataRow("right")]
    [DataRow("none")]
    public void FloatKeywordLegal_SetsBoxFloat(string keyword)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap($"<div id='target' style='float: {keyword}'>content</div>"));

        var target = LayoutHarness.FindById(root, "target");

        Assert.IsNotNull(target);
        Assert.AreEqual(keyword, target.Float);
    }
}
