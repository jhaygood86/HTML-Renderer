using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/FlexPropertyTests.cs (source class <c>FlexPropertyTests</c>).
/// Pure CSSOM parse tests exercising the flex/align/justify shorthand and longhand
/// <c>StyleDeclaration</c> accessors. Distinct from <see cref="FlexboxTests"/>, which was ported from
/// PeachPDF.Tests/CSS/Flexbox.cs and covers legal-value matrices for these same properties in depth -
/// these two tests were verified not to duplicate anything already asserted there.
/// </summary>
[TestClass]
public sealed class FlexPropertyTests
{
    [TestMethod]
    public void JustifyAlign_Parses()
    {
        const string css = """
                            html {
                                justify-content: center;
                                align-items: center;
                                align-content: center;
                                align-self: center;
                            }
                            """;

        var stylesheet = CssConstructionFunctions.ParseStyleSheet(css);
        var info = (StyleRule)stylesheet.StyleRules.First();

        Assert.AreEqual("center", info.Style.AlignItems);
        Assert.AreEqual("center", info.Style.AlignContent);
        Assert.AreEqual("center", info.Style.AlignSelf);
        Assert.AreEqual("center", info.Style.JustifyContent);
    }

    [TestMethod]
    public void FlexAuto_Parses()
    {
        const string css = """
                            html {
                                flex: auto;
                            }
                            """;

        var stylesheet = CssConstructionFunctions.ParseStyleSheet(css);
        var info = (StyleRule)stylesheet.StyleRules.First();

        Assert.AreEqual("1 1 auto", info.Style.Flex);
    }
}
