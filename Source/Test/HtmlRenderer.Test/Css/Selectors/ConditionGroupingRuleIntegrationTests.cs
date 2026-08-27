using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css.Selectors;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/ConditionGroupingRuleIntegrationTests.cs. <c>@supports</c> and
/// <c>@container</c> parse and have working condition-evaluation code
/// (<see cref="TheArtOfDev.HtmlRenderer.Core.CssEngine.SupportsRule"/>,
/// <see cref="TheArtOfDev.HtmlRenderer.Core.CssEngine.ContainerRule"/>,
/// <see cref="TheArtOfDev.HtmlRenderer.Core.CssEngine.DeclarationCondition"/>), but
/// <c>TheArtOfDev.HtmlRenderer.Core.CssData.cs:330-332</c> explicitly documents that the real per-box
/// cascade NEVER applies their inner rules, regardless of whether the condition is true or false - this
/// is a deliberate, still-open scope gap (unlike PeachPDF, which genuinely evaluates and applies both).
///
/// Only the four cases that assert the inner rule DOES apply are ported, as <see cref="Ignore"/>d - they
/// would genuinely fail against the real engine today. The mirror-image "does NOT apply" cases (and the
/// AND/OR combination case, which also expects the block does not apply) are NOT ported: they'd pass,
/// but only vacuously (inner rules never apply here regardless of the condition), so they're not
/// meaningful regression coverage. <c>Supports_SvgOnlyProperty_AppliesToAnInlineSvgElement</c> is not
/// ported either - HTML-Renderer has no SVG rendering pipeline.
/// </summary>
[TestClass]
public sealed class ConditionGroupingRuleIntegrationTests
{
    private static readonly RColor Blue = RColor.FromArgb(0, 0, 255);

    [TestMethod]
    [Ignore("not yet spec compliant - CssData.cs:330-332 documents that the real per-box cascade never " +
            "applies @supports'/@container's inner rules, regardless of the condition. See class doc comment.")]
    public void Supports_InnerRules_Apply_WhenConditionIsSupported()
    {
        var html = Html(
            "p { color: #ff0000; } @supports (display: flex) { p { color: #0000ff; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(Blue, box.ActualColor);
    }

    [TestMethod]
    [Ignore("not yet spec compliant - CssData.cs:330-332 documents that the real per-box cascade never " +
            "applies @supports'/@container's inner rules, regardless of the condition. See class doc comment.")]
    public void SupportsNot_Fallback_Applies_WhenFeatureIsUnsupported()
    {
        var html = Html(
            "@supports not (animation-name: spin) { p { color: #0000ff; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(Blue, box.ActualColor);
    }

    // The exact scenario the pre-evaluation stopgap PeachPDF#283 was written to avoid: an enhanced
    // block and its not-guarded fallback wrapping the same declaration, both present in one
    // stylesheet. Real evaluation must let exactly one of them win, never both and never neither.
    [TestMethod]
    [Ignore("not yet spec compliant - CssData.cs:330-332 documents that the real per-box cascade never " +
            "applies @supports'/@container's inner rules, regardless of the condition. See class doc comment.")]
    public void SupportsEnhancedBlockAndNotGuardedFallback_ExactlyOneApplies()
    {
        var html = Html(
            "@supports (display: flex) { p { color: #0000ff; } } " +
            "@supports not (display: flex) { p { color: #00ff00; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(Blue, box.ActualColor);
        Assert.AreNotEqual(RColor.FromArgb(0, 255, 0), box.ActualColor);
    }

    [TestMethod]
    [Ignore("not yet spec compliant - CssData.cs:330-332 documents that the real per-box cascade never " +
            "applies @supports'/@container's inner rules, regardless of the condition. See class doc comment.")]
    public void Container_InnerRules_Apply_WhenAnEligibleAncestorContainerMatches()
    {
        var html = Html(
            "#box { container-type: inline-size; width: 300px; } " +
            "@container (min-width: 200px) { p { color: #0000ff; } }",
            "<div id=\"box\"><p>text</p></div>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(Blue, box.ActualColor);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

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
