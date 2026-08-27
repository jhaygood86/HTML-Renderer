using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/Property.cs (the closing <c>CssPropertyFactoryCalls</c>/
/// <c>CssUnknownPropertyPreservesCase</c> cases), exercising <c>StyleDeclaration.CreateProperty</c>/
/// <c>SetProperty</c> (Model/StyleDeclaration.cs) and <c>PropertyFactory.Instance</c>
/// (Factories/PropertyFactory.cs) directly rather than through <c>ParseDeclaration</c>.
///
/// <c>StyleDeclaration.CreateProperty</c> (StyleDeclaration.cs lines 198-207) first returns any existing
/// declaration of that name, then falls back to <c>PropertyFactory.Instance.Create(name)</c>
/// (PropertyFactory.cs line 578: longhand, then shorthand, then custom-property lookup) - and only
/// synthesizes a same-named <c>UnknownProperty</c> when the parser is not in strict mode
/// (<c>IsStrictMode</c>, StyleDeclaration.cs line 312, is true whenever
/// <c>StylesheetParser.Options.IncludeUnknownDeclarations</c> is false, which is the default
/// <c>new StylesheetParser()</c> constructor's behavior) - so an unrecognized name under the default
/// parser returns null, matching PeachPDF's own assertion.
/// </summary>
[TestClass]
public sealed class PropertyFactoryTests
{
    [TestMethod]
    public void CssPropertyFactoryCalls()
    {
        var parser = new StylesheetParser();
        var decl = new StyleDeclaration(parser);
        var invalid = decl.CreateProperty("invalid");
        var border = decl.CreateProperty("border");
        var color = decl.CreateProperty("color");
        decl.SetProperty(color);
        var colorAgain = decl.CreateProperty("color");

        Assert.IsNull(invalid);
        Assert.IsNotNull(border);
        Assert.IsNotNull(color);
        Assert.IsNotNull(colorAgain);

        Assert.IsInstanceOfType<BorderProperty>(border);
        Assert.IsInstanceOfType<ColorProperty>(color);
        Assert.AreEqual(color, colorAgain);
    }

    [TestMethod]
    public void CssUnknownPropertyPreservesCase()
    {
        var snippet = "my-Property: something";
        var property = ParseDeclaration(snippet, includeUnknownDeclarations: true);
        Assert.AreEqual("my-Property", property.Name);
        Assert.IsInstanceOfType<UnknownProperty>(property);
    }
}
