using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/MediaFeatures.cs (<c>CssMediaFeaturesTests</c>). Only
/// <c>CssMediaFeatureFactory</c> has real assertions in the PeachPDF source - the other three facts
/// (<c>CssMediaWidthValidation</c>, <c>CssMediaMaxHeightValidation</c>, <c>CssMediaMinDeviceWidthValidation</c>,
/// <c>CssMediaAspectRatio</c>) have fully commented-out bodies there and are not ported.
/// <c>MediaFeatureFactory.Instance.Create</c> plus <c>IsMaximum</c>/<c>IsMinimum</c> match HTML-Renderer's
/// <c>Factories/MediaFeatureFactory.cs</c>.
/// </summary>
[TestClass]
public sealed class MediaFeaturesTests
{
    [TestMethod]
    public void CssMediaFeatureFactory()
    {
        var aspectRatio = MediaFeatureFactory.Instance.Create(FeatureNames.AspectRatio);
        Assert.IsNotNull(aspectRatio);
        Assert.IsInstanceOfType<AspectRatioMediaFeature>(aspectRatio);
        Assert.IsFalse(aspectRatio.IsMaximum);
        Assert.IsFalse(aspectRatio.IsMinimum);

        var colorIndex = MediaFeatureFactory.Instance.Create(FeatureNames.ColorIndex);
        Assert.IsNotNull(colorIndex);
        Assert.IsInstanceOfType<ColorIndexMediaFeature>(colorIndex);
        Assert.IsFalse(colorIndex.IsMaximum);
        Assert.IsFalse(colorIndex.IsMinimum);

        var deviceWidth = MediaFeatureFactory.Instance.Create(FeatureNames.DeviceWidth);
        Assert.IsNotNull(deviceWidth);
        Assert.IsInstanceOfType<DeviceWidthMediaFeature>(deviceWidth);
        Assert.IsFalse(deviceWidth.IsMaximum);
        Assert.IsFalse(deviceWidth.IsMinimum);

        var monochrome = MediaFeatureFactory.Instance.Create(FeatureNames.MaxMonochrome);
        Assert.IsNotNull(monochrome);
        Assert.IsInstanceOfType<MonochromeMediaFeature>(monochrome);
        Assert.IsTrue(monochrome.IsMaximum);
        Assert.IsFalse(monochrome.IsMinimum);

        var illegal = MediaFeatureFactory.Instance.Create("illegal");
        Assert.IsNull(illegal);
    }
}
