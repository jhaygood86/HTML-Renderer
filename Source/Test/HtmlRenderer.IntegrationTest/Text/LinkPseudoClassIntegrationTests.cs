using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.Text;

/// <summary>
/// Verifies <c>:link</c>/<c>:hover</c> pseudo-class handling, and locks in the resulting gap that
/// <c>:visited</c>/<c>:active</c> never match.
/// </summary>
/// <remarks>
/// <para>
/// HTML-Renderer fact (confirmed, <c>Parse\CssParser.cs</c> <c>ParseCssBlockImp</c>): only <c>:link</c> and
/// <c>:hover</c> pseudo-classes are recognized at all during selector parsing - any other pseudo-class
/// (<c>:visited</c>, <c>:active</c>, <c>:focus</c>, <c>:nth-child</c>, ...) makes the whole rule fail its
/// "recognized pseudo-class" check and the ENTIRE css block is dropped during parsing, not just the
/// pseudo-class stripped.
/// </para>
/// <para>
/// A closer trace of <c>a:link</c> specifically (confirmed, <c>Parse\CssParser.cs</c>
/// <c>ParseCssBlockImp</c> + <c>Parse\DomParser.cs</c> <c>IsBlockAssignableToBox</c>): the pseudo-class
/// token is stripped off the selector text before the block is built and is only ever remembered as a
/// boolean "Hover" flag on the resulting <c>CssBlock</c> - so <c>a:link</c> and a plain <c>a</c> selector
/// produce IDENTICAL <c>CssBlock</c> objects (<c>Class="a"</c>, <c>Selectors=null</c>, <c>Hover=false</c>),
/// and whatever a plain <c>a</c> selector matches, <c>a:link</c> matches too - no more, no less. And a
/// plain, non-hierarchical <c>a</c> selector is ALSO already special-cased in
/// <c>DomParser.IsBlockAssignableToBox</c>: <c>box.HtmlTag.Name == "a" &amp;&amp; block.Class == "a" &amp;&amp;
/// !box.HtmlTag.HasAttribute("href")</c> sets assignable=false. So this fork's simple <c>a</c> selector
/// already requires an href, and <c>a:link</c> inherits that requirement too - coincidentally matching
/// :link's CSS-spec href-gated semantics, though not through anything actually keyed off the ":link" token
/// itself. None of the cases below need [Ignore] as a result: each still passes, just via a different
/// mechanism than PeachPDF assumed (which ties :link explicitly to href-presence and drops
/// :visited/:active as a deliberate design choice rather than a parser limitation).
/// </para>
/// <para>
/// Verification update: this trace was checked empirically by running the suite, and the mechanism above is
/// correct - but the FIRST run of these tests showed <c>Link_MatchesAnchorWithHref</c> failing anyway, for a
/// completely unrelated reason: the original marker color, <c>rgb(1,2,3)</c>, is exactly 10 characters, and
/// <c>CssValueParser.TryGetColor</c> (<c>Core/Parse/CssValueParser.cs</c> ~313) requires <c>length &gt; 10</c>
/// before it will even attempt to parse an <c>rgb(...)</c> token - so any all-single-digit-component triple
/// silently fails <c>IsColorValid</c> and the property is dropped before the selector-matching logic above
/// ever gets a chance to matter. Confirmed by direct instrumentation of <c>DomParser.CascadeParseStyles</c>.
/// Switching the marker color to <c>rgb(10,20,30)</c> (11 characters) made every case in this file pass,
/// confirming the mechanism trace above was right all along.
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

        Assert.AreEqual("rgb(10,20,30)", a.Color);
    }

    [TestMethod]
    public void Link_DoesNotMatchAnchorWithoutHref()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<style>a:link { color: rgb(10,20,30) }</style><a>not a link</a>"));
        var a = FindByTag(root, "a")!;

        Assert.AreNotEqual("rgb(10,20,30)", a.Color);
    }

    [TestMethod]
    public void Link_DoesNotMatchNonAnchorElement()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<style>:link { color: rgb(10,20,30) }</style><span id='s' href='https://example.com'>not an anchor</span>"));
        var s = LayoutHarness.FindById(root, "s")!;

        Assert.AreNotEqual("rgb(10,20,30)", s.Color);
    }

    [TestMethod]
    public void Visited_NeverMatches_ByDesign()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<style>a:visited { color: rgb(10,20,30) }</style><a href='https://example.com'>link</a>"));
        var a = FindByTag(root, "a")!;

        Assert.AreNotEqual("rgb(10,20,30)", a.Color);
    }

    [TestMethod]
    public void Active_NeverMatches_ByDesign()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<style>a:active { color: rgb(10,20,30) }</style><a href='https://example.com'>link</a>"));
        var a = FindByTag(root, "a")!;

        Assert.AreNotEqual("rgb(10,20,30)", a.Color);
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
