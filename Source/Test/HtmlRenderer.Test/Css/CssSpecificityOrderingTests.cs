using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/CssSpecificityOrderingTests.cs.
/// PeachPDF's <c>CssData</c> resolves matched rules in (specificity ascending, true document order),
/// a real CSS specificity computation. HTML-Renderer's <see cref="TheArtOfDev.HtmlRenderer.Core.CssData"/>
/// has no specificity concept at all: <see cref="TheArtOfDev.HtmlRenderer.Core.CssData.AddCssBlock"/>
/// buckets blocks by selector class name and, within a bucket, always keeps selector-less ("general")
/// blocks first and hierarchical-selector blocks appended in parse order - id/class/tag specificity and
/// true cross-bucket source order (e.g. across the plain-rule/@media boundary) are not modeled. On top
/// of that, <c>@media</c>-scoped rules parsed by <see cref="TheArtOfDev.HtmlRenderer.Core.Parse.CssParser"/>
/// are stored under their own media key (see <c>ParseMediaStyleBlocks</c>) and are never looked up by
/// <see cref="TheArtOfDev.HtmlRenderer.Core.Parse.DomParser"/> (which only ever calls
/// <c>CssData.GetCssBlock(name)</c>, implicitly querying the "all" media bucket) - so an "@media print"
/// rule never actually applies, regardless of specificity.
/// All four tests are therefore marked <c>[Ignore]</c>: they document the target cascade-ordering
/// behavior (mirroring the PeachPDF regression tests) for when/if real CSS specificity is implemented,
/// but do not pass against the current engine.
/// </summary>
[TestClass]
public sealed class CssSpecificityOrderingTests
{
    [TestMethod]
    [Ignore("not yet spec compliant")]
    public void HigherSpecificityRule_WinsEvenWhenDeclaredEarlier()
    {
        // #el (id, highest specificity) is declared FIRST; div (type, lowest specificity) is
        // declared SECOND. Under HTML-Renderer's actual bucket-order behavior, whichever bucket
        // ("#el" vs "div") the box happens to pick up last during CascadeApplyStyles wins - not
        // specificity. Specificity must decide this, not declaration/bucket order.
        var html = Html("#el { color: #0000ff; } div { color: #ff0000; }", "<div id='el'>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual(RColor.FromArgb(0, 0, 255), box.ActualColor);
    }

    [TestMethod]
    [Ignore("not yet spec compliant")]
    public void EqualSpecificity_SameOrigin_StillResolvesByLastDeclared()
    {
        var html = Html("div { color: #ff0000; } div { color: #0000ff; }", "<div>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual(RColor.FromArgb(0, 0, 255), box.ActualColor);
    }

    [TestMethod]
    [Ignore("not yet spec compliant")]
    public void MediaRuleDeclaredEarlier_LosesToEqualSpecificityPlainRuleDeclaredLater()
    {
        // The @media print block (containing a "div" rule) appears FIRST in the source; a plain
        // "div" rule appears SECOND. In HTML-Renderer @media rules are parsed into a separate
        // media bucket that DomParser never queries at all, so this can never resolve to the
        // media rule's color no matter the ordering - it documents the target source-order
        // behavior once @media support (and specificity) exist.
        var html = Html(
            "@media print { div { color: #0000ff; } } div { color: #ff0000; }",
            "<div>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual(RColor.FromArgb(255, 0, 0), box.ActualColor);
    }

    [TestMethod]
    [Ignore("not yet spec compliant")]
    public void CommaListRule_UsesOnlyTheMatchedBranchsSpecificity_NotASum()
    {
        // The box matches ".a" (one class) but NOT "#b" (an id) in the list selector ".a, #b".
        // HTML-Renderer's CssParser.FeedStyleBlock splits a comma-separated selector list into
        // independent CssBlocks per class up front (one bucketed under ".a", another under "#b"),
        // so there is no combined/summed specificity to get wrong in the first place - but there
        // is also no real specificity computation to correctly outrank ".a.c" (two classes)
        // either. Documents the target: ".a.c" (two classes) must win.
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
