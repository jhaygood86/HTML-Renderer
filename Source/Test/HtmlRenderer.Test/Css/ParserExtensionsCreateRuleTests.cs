using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/ParserExtensionsCreateRuleTests.cs. <c>ParserExtensions.CreateRule</c> is a
/// near-verbatim match with HTML-Renderer's <c>Model/ParserExtensions.cs</c>: the same 13 <see cref="RuleType"/>
/// cases construct a concrete rule, and the same 4 types with no concrete subtype (<see cref="RuleType.Unknown"/>,
/// <see cref="RuleType.RegionStyle"/>, <see cref="RuleType.FontFeatureValues"/>, <see cref="RuleType.CounterStyle"/>)
/// fall through to <see langword="null"/>.
/// </summary>
[TestClass]
public sealed class ParserExtensionsCreateRuleTests
{
    [TestMethod]
    [DataRow((byte)RuleType.Charset, typeof(CharsetRule))]
    [DataRow((byte)RuleType.Document, typeof(DocumentRule))]
    [DataRow((byte)RuleType.FontFace, typeof(FontFaceRule))]
    [DataRow((byte)RuleType.Import, typeof(ImportRule))]
    [DataRow((byte)RuleType.Keyframe, typeof(KeyframeRule))]
    [DataRow((byte)RuleType.Keyframes, typeof(KeyframesRule))]
    [DataRow((byte)RuleType.Media, typeof(MediaRule))]
    [DataRow((byte)RuleType.Container, typeof(ContainerRule))]
    [DataRow((byte)RuleType.Namespace, typeof(NamespaceRule))]
    [DataRow((byte)RuleType.Page, typeof(PageRule))]
    [DataRow((byte)RuleType.Style, typeof(StyleRule))]
    [DataRow((byte)RuleType.Supports, typeof(SupportsRule))]
    [DataRow((byte)RuleType.Viewport, typeof(ViewportRule))]
    public void CreateRule_KnownRuleType_ReturnsExpectedRuleSubtype(byte typeValue, Type expectedType)
    {
        var parser = new StylesheetParser();

        var rule = parser.CreateRule((RuleType)typeValue);

        Assert.IsNotNull(rule);
        Assert.IsInstanceOfType(rule, expectedType);
    }

    [TestMethod]
    [DataRow((byte)RuleType.Unknown)]
    [DataRow((byte)RuleType.RegionStyle)]
    [DataRow((byte)RuleType.FontFeatureValues)]
    [DataRow((byte)RuleType.CounterStyle)]
    public void CreateRule_RuleTypeWithNoSubtype_ReturnsNull(byte typeValue)
    {
        var parser = new StylesheetParser();

        Assert.IsNull(parser.CreateRule((RuleType)typeValue));
    }
}
