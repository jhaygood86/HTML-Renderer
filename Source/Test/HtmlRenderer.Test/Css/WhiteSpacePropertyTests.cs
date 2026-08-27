using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/WhiteSpaceProperty.cs.
/// PeachPDF.CSS models "white-space" as a typed, validating <c>WhiteSpaceProperty</c> (legal keywords are
/// stored, illegal ones are rejected). HTML-Renderer's CSS engine port added the same real, validating
/// <c>WhiteSpaceProperty</c> (see Source/HtmlRenderer/Core/CssEngine/StyleProperties/Text/WhiteSpaceProperty.cs) -
/// the old raw-string, unfiltered <c>CssData.GetCssBlock</c> pass-through this file previously described no
/// longer exists at all. Exercised here through <see cref="CssParser.ParseInlineStyle"/>, whose resulting
/// <c>StyleDeclaration</c> only stores a value that actually parsed as a legal keyword.
/// WhiteSpaceInvalidKeywordIllegal, previously <c>[Ignore]</c>d because the old parser stored any raw token
/// verbatim, now genuinely passes against the real validating property and is un-ignored.
/// </summary>
[TestClass]
public sealed class WhiteSpacePropertyTests
{
    private static string GetWhiteSpace(string declaration)
    {
        var rule = new CssParser(new MockAdapter()).ParseInlineStyle(declaration);
        Assert.IsNotNull(rule);
        return rule.Style["white-space"];
    }

    [TestMethod]
    [DataRow("normal")]
    [DataRow("pre")]
    [DataRow("nowrap")]
    [DataRow("pre-wrap")]
    [DataRow("pre-line")]
    public void WhiteSpaceKeywordLegal(string keyword)
    {
        Assert.AreEqual(keyword, GetWhiteSpace($"white-space: {keyword}"));
    }

    [TestMethod]
    public void WhiteSpaceInvalidKeywordIllegal()
    {
        Assert.IsTrue(string.IsNullOrEmpty(GetWhiteSpace("white-space: wavy")));
    }
}
