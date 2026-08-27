using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/RowGapProperty.cs.
/// RowGapLegalValues (length/percent/global keywords) is ported as a plain passing test.
/// RowGapAcceptsNormalKeyword is ported under [Ignore] - HTML-Renderer's
/// <see cref="RowGapProperty"/> (Source/HtmlRenderer/Core/CssEngine/StyleProperties/RowGapProperty.cs:5)
/// uses <c>Converters.LengthOrPercentConverter.OrGlobalValue().OrDefault(0)</c>, while PeachPDF's
/// RowGapProperty.cs:5 uses <c>Converters.LengthOrPercentOrNormalConverter...</c> instead;
/// <c>LengthOrPercentOrNormalConverter</c> does not exist anywhere under Source/HtmlRenderer (confirmed
/// via grep), so "row-gap: normal" is silently rejected here.
/// </summary>
[TestClass]
public sealed class RowGapPropertyTests
{
    [TestMethod]
    [DataRow("0")]
    [DataRow("20px")]
    [DataRow("1em")]
    [DataRow("3vmin")]
    [DataRow("0.5cm")]
    [DataRow("10%")]
    [DataRow("inherit")]
    [DataRow("initial")]
    [DataRow("revert")]
    [DataRow("revert-layer")]
    [DataRow("unset")]
    public void RowGapLegalValues(string value)
        => CssConstructionFunctions.TestForLegalValue<RowGapProperty>(PropertyNames.RowGap, value);

    [TestMethod]
    [Ignore("not yet spec compliant")]
    public void RowGapAcceptsNormalKeyword()
        => CssConstructionFunctions.TestForLegalValue<RowGapProperty>(PropertyNames.RowGap, "normal");
}
