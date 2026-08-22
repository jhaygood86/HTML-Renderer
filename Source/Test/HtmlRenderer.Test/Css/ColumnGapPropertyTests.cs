using System.Collections.Generic;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>Ported from PeachPDF.Tests/CSS/PropertyTests/ColumnGapProperty.cs. Pure CSSOM parse tests - no layout involved.</summary>
[TestClass]
public sealed class ColumnGapPropertyTests
{
    [TestMethod]
    [DynamicData(nameof(ColumnGapTestDataValues))]
    public void ColumnGapLegalValues(string value)
        => CssConstructionFunctions.TestForLegalValue<ColumnGapProperty>(PropertyNames.ColumnGap, value);

    [Ignore("not yet spec compliant: ColumnGapProperty's converter (StyleProperties/Columns/ColumnGapProperty.cs) " +
            "is Converters.LengthOrPercentConverter.OrGlobalValue().OrDefault(1em) - there is no 'normal'-accepting " +
            "branch anywhere in the chain, so 'normal' is rejected instead of accepted as the spec's default keyword.")]
    [TestMethod]
    public void ColumnGapAcceptsNormalKeyword()
        => CssConstructionFunctions.TestForLegalValue<ColumnGapProperty>(PropertyNames.ColumnGap, "normal");

    public static IEnumerable<object[]> ColumnGapTestDataValues => CssConstructionFunctions.LengthOrPercentOrGlobalTestValues;
}
