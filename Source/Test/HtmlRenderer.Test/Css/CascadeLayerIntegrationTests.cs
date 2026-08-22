using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/CascadeLayerIntegrationTests.cs.
/// Cascade-layer (<c>@layer</c>) support, per CSS Cascade 5. HTML-Renderer's <c>CssData.IndexRules</c>
/// (Core/CssData.cs:321-329) indexes an <c>@layer</c> block's contents as ordinary, unlayered rules -
/// they apply, ordered by source position and specificity - but true layer precedence, the
/// <c>!important</c> layer-order reversal, and layer-aware <c>revert-layer</c> are deliberately not
/// implemented (that needs a layer-banded cascade, tracked as separate work). <c>revert-layer</c> itself
/// parses but is routed through the exact same branch as plain <c>revert</c> (see
/// Core/Parse/DomParser.cs, e.g. lines 404/459/514: <c>prop.Value == CssConstants.Revert || prop.Value ==
/// CssConstants.RevertLayer</c>), so it rolls all the way back to the origin rather than to a lower layer.
/// The tests below split accordingly: cases where ordinary specificity/document-order tie-breaking
/// happens to produce the spec-correct answer anyway are ported as real (passing) tests; cases that
/// actually require layer-banded precedence are <c>[Ignore]</c>d. One PeachPDF test
/// (<c>NestedSublayer_VsParentDirectRules_CharacterizesCurrentOrdering</c>) is a characterization test
/// pinning a PeachPDF-specific implementation detail with no HTML-Renderer counterpart and is not ported.
/// </summary>
[TestClass]
public sealed class CascadeLayerIntegrationTests
{
    private const string LayerPrecedenceNotImplemented =
        "HTML-Renderer's CssData.IndexRules (Core/CssData.cs:321-329) indexes @layer contents as ordinary, " +
        "unlayered rules - true layer precedence, the !important layer-order reversal, and layer-aware " +
        "revert-layer are deliberately not implemented (needs a layer-banded cascade).";

    private const string RevertLayerNotImplemented =
        "revert-layer parses but is routed through the same branch as plain revert (see " +
        "Source/HtmlRenderer/Core/Parse/DomParser.cs, e.g. lines 404/459/514), so it rolls back to the " +
        "origin rather than to a lower layer. See also Core/CssData.cs:321-329.";

    [TestMethod]
    public void LayerBlock_Rules_AreApplied_NotDropped()
    {
        var html = Html(
            "@layer base { p { color: #0000ff; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    public void LaterLayer_Wins_OverEarlierLayer_AtEqualSpecificity()
    {
        // base is declared before components; both rules have identical (type-selector) specificity, so
        // the ordinary "last declared wins" tie-break already gives the spec-correct answer here, even
        // without real layer precedence.
        var html = Html(
            "@layer base { p { color: #ff0000; } } @layer components { p { color: #0000ff; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    [Ignore(LayerPrecedenceNotImplemented)]
    public void LaterLayer_Wins_EvenAgainstHigherSpecificityInEarlierLayer()
    {
        // The earlier layer's rule has higher specificity (id), but layer precedence sorts ahead of
        // specificity: the later layer's low-specificity rule should still win. Without real layer
        // precedence, ordinary specificity lets the higher-specificity id rule win instead.
        var html = Html(
            "@layer a { #x { color: #ff0000; } } @layer b { p { color: #0000ff; } }",
            "<p id='x'>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    [Ignore(LayerPrecedenceNotImplemented)]
    public void UnlayeredRule_Wins_OverAnyLayeredRule()
    {
        // An unlayered normal declaration should outrank a layered one regardless of specificity. Without
        // real layer precedence, ordinary specificity lets the higher-specificity layered id rule win.
        var html = Html(
            "@layer utilities { p#x { color: #ff0000; } } p { color: #0000ff; }",
            "<p id='x'>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    [Ignore(LayerPrecedenceNotImplemented)]
    public void StatementDeclaresOrder_RegardlessOfBlockOrder()
    {
        // The leading "@layer base, utilities;" statement should fix the order (utilities after base), so
        // utilities wins even though its block is written BEFORE base's block in source. Without real
        // layer-order tracking, this statement contributes no style rules at all (CssData.IndexRules'
        // comment: "@layer statements ... contribute no style rules to the cascade"), so plain document
        // order lets the textually-last block (base) win instead.
        var html = Html(
            "@layer base, utilities; @layer utilities { p { color: #0000ff; } } @layer base { p { color: #ff0000; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    public void WithinOneLayer_SpecificityStillBreaksTies()
    {
        var html = Html(
            "@layer base { p { color: #ff0000; } p.hi { color: #0000ff; } }",
            "<p class='hi'>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    public void DottedLayerName_IsParsedAndApplied()
    {
        // A nested/dotted layer name (`@layer framework.utilities`) parses and its rules apply.
        var html = Html(
            "@layer framework.utilities { p { color: #0000ff; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    public void AnonymousLayer_Rules_AreApplied_AndBeatenByUnlayered()
    {
        // An anonymous @layer { } is its own distinct layer; its rules apply. Both rules here have equal
        // (type-selector) specificity, so ordinary "last declared wins" already gives the spec-correct
        // answer (the unlayered rule, written second) without needing real layer precedence.
        var html = Html(
            "@layer { p { color: #ff0000; } } p { color: #0000ff; }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    // ── !important layer-order reversal (CSS Cascade 5 §6.4.2) ────────────────────

    [TestMethod]
    [Ignore(LayerPrecedenceNotImplemented)]
    public void Important_EarlierLayer_Wins_OverLaterLayer()
    {
        // For NORMAL declarations the later layer wins; among !important declarations the layer order
        // should reverse, so the EARLIER layer (base) should win over the later one (utilities). Without
        // real layer precedence, plain "last !important declaration wins" lets utilities win instead.
        var html = Html(
            "@layer base { p { color: #ff0000 !important; } } @layer utilities { p { color: #0000ff !important; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(255, 0, 0)", box.Color);
    }

    [TestMethod]
    [Ignore(LayerPrecedenceNotImplemented)]
    public void Important_LayeredRule_Wins_OverUnlayeredImportant()
    {
        // An unlayered !important declaration should LOSE to a layered !important one (the mirror of the
        // normal-declaration rule where unlayered wins). Without real layer precedence, plain "last
        // !important declaration wins" lets the unlayered rule (written second) win instead.
        var html = Html(
            "@layer base { p { color: #0000ff !important; } } p { color: #ff0000 !important; }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    public void Important_WithinOneLayer_HigherSpecificityStillWins()
    {
        // The layer reversal is about layer order only — within a single layer, ordinary specificity
        // still decides among !important declarations, with no layer-precedence machinery involved.
        var html = Html(
            "@layer base { p { color: #ff0000 !important; } p.hi { color: #0000ff !important; } }",
            "<p class='hi'>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    // ── Nested sub-layers stay contiguous within their parent's band ──────────────

    [TestMethod]
    [Ignore(LayerPrecedenceNotImplemented)]
    public void NestedSublayers_StayContiguous_WithinParentBand()
    {
        // a first appears (via a.b) before c, so a's WHOLE subtree {a.b, a.d} should rank below c - even
        // though a.d is declared last of all. Without real layer-banded precedence, plain document order
        // lets a.d (textually last) win instead of c.
        var html = Html(
            "@layer a.b { p { color: #ff0000; } } @layer c { p { color: #00ff00; } } @layer a.d { p { color: #0000ff; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 255, 0)", box.Color);
    }

    [TestMethod]
    public void NestedSublayer_LaterWithinParent_Wins_OverEarlierSublayer()
    {
        // Within one parent (a), a later-declared sub-layer (a.d) beats an earlier one (a.b). Both rules
        // have equal (type-selector) specificity, so ordinary "last declared wins" already gives the
        // spec-correct answer here without needing real layer-banded precedence.
        var html = Html(
            "@layer a.b { p { color: #ff0000; } } @layer a.d { p { color: #0000ff; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    // ── Layer-aware revert-layer ────────────────────────────────────────────────

    [TestMethod]
    [Ignore(RevertLayerNotImplemented)]
    public void RevertLayer_RollsBackToLowerLayer_NotToOrigin()
    {
        // revert-layer in the later layer should reveal the earlier layer's value (blue), rather than
        // rolling all the way back to the UA/origin default the way plain `revert` does.
        var html = Html(
            "@layer base { p { color: #0000ff; } } @layer top { p { color: revert-layer; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    public void Revert_StillRollsBackToOrigin_NotToLowerLayer()
    {
        // Regression guard: plain `revert` is unaffected by any of this — it rolls back past the whole
        // author origin, so the earlier layer's blue is NOT revealed (the inherited/initial color applies
        // instead). This holds today regardless of layer-precedence support.
        var html = Html(
            "@layer base { p { color: #0000ff; } } @layer top { p { color: revert; } }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreNotEqual("rgb(0, 0, 255)", box.Color);
    }

    [TestMethod]
    [Ignore(RevertLayerNotImplemented)]
    public void RevertLayer_OnCustomProperty_RollsBackToLowerLayer()
    {
        // revert-layer should work for custom properties too: the consumer resolves to the lower layer's
        // value.
        var html = Html(
            "@layer base { p { --c: #0000ff; } } @layer top { p { --c: revert-layer; } } p { color: var(--c); }",
            "<p>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual("rgb(0, 0, 255)", box.Color);
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
