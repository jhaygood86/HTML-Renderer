using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Text;

/// <summary>
/// Verifies <c>:link</c>/<c>:hover</c> pseudo-class handling, and locks in the resulting gap that
/// <c>:visited</c>/<c>:active</c> never match.
/// </summary>
/// <remarks>
/// <para>
/// The CSS engine port replaced selector matching with a real implementation:
/// <see cref="TheArtOfDev.HtmlRenderer.Core.CssData"/>'s private <c>DoesSelectorMatch(PseudoClassSelector, CssBox)</c>
/// only special-cases three pseudo-classes - <c>:hover</c> (matched structurally, always true; diverted to
/// <c>HtmlContainerInt.AddHoverBox</c> instead of applied directly, so live mouse state stays out of the
/// cascade), <c>:root</c> (matches the document's <c>&lt;html&gt;</c> element), and <c>:link</c> (matches
/// <c>box.IsClickable</c>) - everything else, including <c>:visited</c>/<c>:active</c>/<c>:focus</c>/
/// <c>:nth-child</c>, falls through to <c>return false</c>, so a rule using them never matches anything.
/// Unlike the old parser (which dropped the ENTIRE containing css block when it hit an unrecognized
/// pseudo-class), the real engine parses the rule normally and simply never matches it - same observable
/// outcome for :visited/:active here, via a completely different, real mechanism.
/// </para>
/// <para>
/// <c>:link</c> specifically resolves through <see cref="TheArtOfDev.HtmlRenderer.Core.Dom.CssBox.IsClickable"/>,
/// which is real, spec-correct, and deliberate (not coincidental): only an <c>&lt;a&gt;</c> element carrying an
/// <c>href</c> attribute is clickable, matching CSS Selectors' own href-gated definition of <c>:link</c>
/// directly - an <c>&lt;a&gt;</c> used only as a named anchor/target (no href) is correctly excluded.
/// </para>
/// </remarks>
[DoNotParallelize]
[TestClass]
public sealed class LinkPseudoClassIntegrationTests
{
    // Anchors used to verify :link matching deliberately have no id/name attribute, matching PeachPDF's own
    // setup, so FindByTag (not FindById) locates the anchor in these tests.

    [TestMethod]
    public void Link_MatchesAnchorWithHref()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<style>a:link { color: rgb(10,20,30) }</style><a href='https://example.com'>link</a>"));
        var a = FindByTag(root, "a")!;

        Assert.AreEqual("rgb(10, 20, 30)", a.Color);
    }

    [TestMethod]
    public void Link_DoesNotMatchAnchorWithoutHref()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<style>a:link { color: rgb(10,20,30) }</style><a>not a link</a>"));
        var a = FindByTag(root, "a")!;

        Assert.AreNotEqual("rgb(10, 20, 30)", a.Color);
    }

    [TestMethod]
    public void Link_DoesNotMatchNonAnchorElement()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<style>:link { color: rgb(10,20,30) }</style><span id='s' href='https://example.com'>not an anchor</span>"));
        var s = LayoutHarness.FindById(root, "s")!;

        Assert.AreNotEqual("rgb(10, 20, 30)", s.Color);
    }

    [TestMethod]
    public void Visited_NeverMatches_ByDesign()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<style>a:visited { color: rgb(10,20,30) }</style><a href='https://example.com'>link</a>"));
        var a = FindByTag(root, "a")!;

        Assert.AreNotEqual("rgb(10, 20, 30)", a.Color);
    }

    [TestMethod]
    public void Active_NeverMatches_ByDesign()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<style>a:active { color: rgb(10,20,30) }</style><a href='https://example.com'>link</a>"));
        var a = FindByTag(root, "a")!;

        Assert.AreNotEqual("rgb(10, 20, 30)", a.Color);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static CssBox? FindByTag(CssBox box, string tag)
    {
        if (box.HtmlTag?.Name.Equals(tag, System.StringComparison.OrdinalIgnoreCase) == true)
            return box;
        foreach (var child in box.Boxes)
        {
            var found = FindByTag(child, tag);
            if (found != null) return found;
        }
        return null;
    }
}
