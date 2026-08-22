using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/CssSpecificityOrderingTests.cs.
/// PeachPDF's <c>CssData</c> resolves matched rules in (specificity ascending, true document order), a
/// real CSS specificity computation. HTML-Renderer's old <c>CssData</c> had no specificity concept at
/// all - selector matching was bucketed by class name with no id/class/tag specificity and no true
/// cross-bucket source order, and <c>@media</c>-scoped rules were stored under a separate key that
/// <c>DomParser</c> never queried, so an "@media print" rule never actually applied regardless of
/// specificity. The CSS engine port replaced all of that with the real thing: <see cref="TheArtOfDev.HtmlRenderer.Core.CssData.GetStyleRulesByOrigin"/>
/// (via <c>GetMatchedSpecificity</c>) orders matched rules by real CSS specificity, tie-broken by true
/// document order (assigned across the plain-rule/@media boundary by <c>IndexRules</c>), and <c>@media</c>
/// is evaluated for real (see <see cref="TheArtOfDev.HtmlRenderer.Core.MediaQueryMatcher"/>). All four
/// cases below now genuinely pass against the real engine and are un-ignored.
/// </summary>
[TestClass]
public sealed class CssSpecificityOrderingTests
{
    [TestMethod]
    public void HigherSpecificityRule_WinsEvenWhenDeclaredEarlier()
    {
        // #el (id, highest specificity) is declared FIRST; div (type, lowest specificity) is
        // declared SECOND. Specificity decides this, not declaration order.
        var html = Html("#el { color: #0000ff; } div { color: #ff0000; }", "<div id='el'>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual(RColor.FromArgb(0, 0, 255), box.ActualColor);
    }

    [TestMethod]
    public void EqualSpecificity_SameOrigin_StillResolvesByLastDeclared()
    {
        var html = Html("div { color: #ff0000; } div { color: #0000ff; }", "<div>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual(RColor.FromArgb(0, 0, 255), box.ActualColor);
    }

    [TestMethod]
    public void MediaRuleDeclaredEarlier_LosesToEqualSpecificityPlainRuleDeclaredLater()
    {
        // The @media print block (containing a "div" rule) appears FIRST in the source; a plain
        // "div" rule appears SECOND. LayoutHarness's default MockAdapter reports "screen" media, so
        // the @media print rule doesn't apply at all here regardless of source order - this also
        // exercises that a non-matching-media rule is correctly excluded even at equal specificity.
        var html = Html(
            "@media print { div { color: #0000ff; } } div { color: #ff0000; }",
            "<div>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual(RColor.FromArgb(255, 0, 0), box.ActualColor);
    }

    [TestMethod]
    public void CommaListRule_UsesOnlyTheMatchedBranchsSpecificity_NotASum()
    {
        // The box matches ".a" (one class) but NOT "#b" (an id) in the list selector ".a, #b" - per
        // GetMatchedSpecificity, a matched list selector's effective specificity is whichever
        // alternative actually matched (".a"), not a summed/static max across the whole list, so
        // ".a.c" (two classes - higher specificity than a single class) correctly wins.
        var html = Html(
            ".a, #b { color: #0000ff; } .a.c { color: #ff0000; }",
            "<div class='a c'>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual(RColor.FromArgb(255, 0, 0), box.ActualColor);
    }

    // ── Helpers (mirrors PeachPDF's SelectorMatchingTests.cs conventions, adapted to the
    //    synchronous LayoutHarness used across this test project) ────────────────

    private static string Html(string css, string body) =>
        $"<!DOCTYPE html><html><head><style>{css}</style></head><body>{body}</body></html>";

    private static CssBox FindBoxByTag(string html, string tag)
    {
        var (root, _) = LayoutHarness.Layout(html);
        var box = LayoutHarness.Descendants(root).FirstOrDefault(b => b.HtmlTag != null && b.HtmlTag.Name == tag);
        Assert.IsNotNull(box);
        return box!;
    }
}
