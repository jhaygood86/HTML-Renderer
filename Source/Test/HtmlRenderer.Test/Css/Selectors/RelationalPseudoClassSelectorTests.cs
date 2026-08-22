using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css.Selectors;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/RelationalPseudoClassSelectorTests.cs. :not(), :is()/:matches(), and
/// :where() - plus plain (descendant-form) :has(S) - are all faithful ports with matching specificity
/// semantics (see <see cref="TheArtOfDev.HtmlRenderer.Core.CssEngine.NotSelector"/>,
/// <see cref="TheArtOfDev.HtmlRenderer.Core.CssEngine.MatchesSelector"/>,
/// <see cref="TheArtOfDev.HtmlRenderer.Core.CssEngine.HasSelector"/>), so those are ported here via the
/// <see cref="LayoutHarness"/>/<see cref="MockAdapter"/> pattern (see
/// <see cref="CssSpecificityOrderingTests"/>).
///
/// The four leading-combinator ":has()" matching tests (":has(&gt; S)"/":has(+ S)"/":has(~ S)" and the
/// mixed-combinator comma-list case) are ported below as <see cref="Ignore"/>d: HTML-Renderer's
/// <c>HasSelector</c> only supports the default (descendant) relative-selector form - there is no
/// <c>Selectors/RelativeSelector.cs</c>. The root cause is
/// <see cref="TheArtOfDev.HtmlRenderer.Core.CssEngine.SelectorConstructor"/>'s private
/// <c>Insert(ISelector)</c> (lines 362-387): when a combinator token arrives before any selector has
/// been accumulated yet (i.e. it's the leading token inside "S" of "&gt;(S)"), <c>_temp</c> is still
/// null, so the "no previous selector" branch runs <c>_combinators.Clear()</c> and simply assigns
/// <c>_temp = selector</c> - silently discarding the leading combinator instead of attaching it. See
/// also <c>HasSelector</c>'s own doc comment, which documents this as a known, intentional v1 gap.
/// </summary>
[TestClass]
public sealed class RelationalPseudoClassSelectorTests
{
    // ── :not() ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public void Not_ExcludesElementsMatchingTheArgument()
    {
        var html = Html(
            "p:not(.exclude) { background-color: #ff0000; }",
            "<div><p class='exclude'>1</p><p>2</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(2, boxes.Count);
        Assert.AreEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
    }

    [TestMethod]
    public void NotNot_IsRejectedAsInvalid_WholeRuleMatchesNothing()
    {
        // The parser has a pre-existing restriction against nesting :not() inside :not() (IsNested
        // flag) - the whole enclosing selector becomes invalid, so the rule matches nothing at all,
        // even for an element that would satisfy the (nonsensical) double-negation.
        var html = Html(
            "p:not(:not(.foo)) { background-color: #ff0000; }",
            "<div><p class='foo'>1</p></div>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(RColor.Transparent, box.ActualBackgroundColor);
    }

    // ── :is() / :matches() ────────────────────────────────────────────────────

    [TestMethod]
    public void Is_MatchesEitherBranch()
    {
        var html = Html(
            "p:is(.a, .b) { background-color: #ff0000; }",
            "<div><p class='a'>1</p><p class='b'>2</p><p>3</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(3, boxes.Count);
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[2].ActualBackgroundColor);
    }

    [TestMethod]
    public void Matches_LegacyAliasBehavesTheSameAsIs()
    {
        var html = Html(
            "p:matches(.a, .b) { background-color: #ff0000; }",
            "<div><p class='a'>1</p><p>2</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(2, boxes.Count);
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
    }

    [TestMethod]
    public void Is_SpecificityIsTheStaticMaxOfItsArguments_NotJustTheMatchedBranch()
    {
        // ":is(.a, #b)" must report specificity = max(.a, #b) = one-id, per spec - even though this
        // box only matches via the ".a" branch (never #b). If :is()'s specificity were instead
        // computed dynamically from only the matched branch (one-class), ".a.c" (two classes) would
        // incorrectly outrank it; with the correct static-max (one-id) behavior, :is(...) wins.
        var html = Html(
            ":is(.a, #b) { color: #0000ff; } .a.c { color: #ff0000; }",
            "<div class='a c'>text</div>");
        var box = FindBoxByTag(html, "div");
        Assert.AreEqual(RColor.FromArgb(0, 0, 255), box.ActualColor);
    }

    // ── :has() ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public void Has_MatchesWhenADirectChildSatisfiesTheArgument()
    {
        var html = Html(
            "div:has(.foo) { background-color: #ff0000; }",
            "<div id='yes'><span class='foo'>x</span></div><div id='no'><span>x</span></div>");
        var yes = FindBoxById(html, "yes");
        var no = FindBoxById(html, "no");
        Assert.AreNotEqual(RColor.Transparent, yes.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, no.ActualBackgroundColor);
    }

    [TestMethod]
    public void Has_MatchesWhenADeeplyNestedDescendantSatisfiesTheArgument()
    {
        var html = Html(
            "div:has(.foo) { background-color: #ff0000; }",
            "<div id='outer'><section><article><span class='foo'>deep</span></article></section></div>");
        var outer = FindBoxById(html, "outer");
        Assert.AreNotEqual(RColor.Transparent, outer.ActualBackgroundColor);
    }

    [TestMethod]
    public void Has_CommaSeparatedArgument_MatchesEitherBranch()
    {
        var html = Html(
            "div:has(.a, .b) { background-color: #ff0000; }",
            "<div id='yes'><span class='b'>x</span></div><div id='no'><span class='c'>x</span></div>");
        var yes = FindBoxById(html, "yes");
        var no = FindBoxById(html, "no");
        Assert.AreNotEqual(RColor.Transparent, yes.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, no.ActualBackgroundColor);
    }

    [TestMethod]
    [Ignore("not yet spec compliant - see class doc comment: HasSelector has no RelativeSelector, so " +
            "a leading combinator inside :has(...) is silently discarded by SelectorConstructor.Insert(ISelector).")]
    public void Has_ChildCombinator_MatchesOnlyDirectChild()
    {
        // ":has(> .foo)" must NOT behave like plain ":has(.foo)" - a .foo that's only a grandchild
        // (nested one level deeper than a direct child) must not satisfy the child-combinator form.
        var html = Html(
            "div:has(> .foo) { background-color: #ff0000; }",
            "<div id='yes'><span class='foo'>x</span></div><div id='no'><section><span class='foo'>x</span></section></div>");
        var yes = FindBoxById(html, "yes");
        var no = FindBoxById(html, "no");
        Assert.AreNotEqual(RColor.Transparent, yes.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, no.ActualBackgroundColor);
    }

    [TestMethod]
    [Ignore("not yet spec compliant - see class doc comment: HasSelector has no RelativeSelector, so " +
            "a leading combinator inside :has(...) is silently discarded by SelectorConstructor.Insert(ISelector).")]
    public void Has_ChildCombinator_WithMultiCompoundArgument_AnchorsOnlyItsFirstCompoundToTheChild()
    {
        // ":has(> .a .b)" means the DIRECT CHILD must match ".a", and ".b" must be a descendant of
        // THAT child - not that the direct child itself must satisfy the whole ".a .b" chain as its
        // own subject (which would require the child to literally match ".b").
        // ".b" is nested two levels below ".a" (not a direct child of it) to exercise the recursive
        // any-depth descendant search for the chain's internal (non-leading) combinator, not just its
        // first level.
        var html = Html(
            "div:has(> .a .b) { background-color: #ff0000; }",
            "<div id='yes'><section class='a'><em><span class='b'>x</span></em></section></div>" +
            "<div id='no'><div><section class='a'><em><span class='b'>x</span></em></section></div></div>" +
            "<div id='no-descendant'><section class='a'><em>no b here</em></section></div>");
        var yes = FindBoxById(html, "yes");
        var no = FindBoxById(html, "no");
        var noDescendant = FindBoxById(html, "no-descendant");
        Assert.AreNotEqual(RColor.Transparent, yes.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, no.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, noDescendant.ActualBackgroundColor);
    }

    [TestMethod]
    [Ignore("not yet spec compliant - see class doc comment: HasSelector has no RelativeSelector, so " +
            "a leading combinator inside :has(...) is silently discarded by SelectorConstructor.Insert(ISelector). " +
            "Additionally, unlike PeachPDF, plain \":has(*)\" here matches a text-only child (verified empirically), " +
            "so this doesn't even hold vacuously once the combinator is dropped.")]
    public void Has_ChildCombinator_WithUniversalSelector_DoesNotMatchATextOnlyChild()
    {
        var html = Html(
            "div:has(> *) { background-color: #ff0000; }",
            "<div id='yes'><span>x</span></div><div id='no'>just text</div>");
        var yes = FindBoxById(html, "yes");
        var no = FindBoxById(html, "no");
        Assert.AreNotEqual(RColor.Transparent, yes.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, no.ActualBackgroundColor);
    }

    [TestMethod]
    [Ignore("not yet spec compliant - see class doc comment: HasSelector has no RelativeSelector, so " +
            "a leading combinator inside :has(...) is silently discarded by SelectorConstructor.Insert(ISelector).")]
    public void Has_AdjacentSiblingCombinator_MatchesOnlyImmediateNextSibling()
    {
        var html = Html(
            "div:has(+ .foo) { background-color: #ff0000; }",
            "<div id='yes'></div><span class='foo'>x</span>" +
            "<div id='no'></div><section></section><span class='foo'>x</span>");
        var yes = FindBoxById(html, "yes");
        var no = FindBoxById(html, "no");
        Assert.AreNotEqual(RColor.Transparent, yes.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, no.ActualBackgroundColor);
    }

    [TestMethod]
    [Ignore("not yet spec compliant - see class doc comment: HasSelector has no RelativeSelector, so " +
            "a leading combinator inside :has(...) is silently discarded by SelectorConstructor.Insert(ISelector).")]
    public void Has_GeneralSiblingCombinator_MatchesAnyFollowingSibling()
    {
        // Same markup shape as the adjacent-sibling test's "no" case (.foo is a later, not immediate,
        // sibling) - "~" must succeed where "+" fails.
        var html = Html(
            "div:has(~ .foo) { background-color: #ff0000; }",
            "<div id='yes'></div><section></section><span class='foo'>x</span>" +
            "<div id='no'></div>");
        var yes = FindBoxById(html, "yes");
        var no = FindBoxById(html, "no");
        Assert.AreNotEqual(RColor.Transparent, yes.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, no.ActualBackgroundColor);
    }

    [TestMethod]
    [Ignore("not yet spec compliant - see class doc comment: HasSelector has no RelativeSelector, so " +
            "a leading combinator inside :has(...) is silently discarded by SelectorConstructor.Insert(ISelector).")]
    public void Has_MixedLeadingCombinatorsInCommaList_MatchesEitherBranch()
    {
        // Each comma-separated alternative tracks its own leading combinator independently.
        var html = Html(
            "div:has(> .a, + .b) { background-color: #ff0000; }",
            "<div id='child'><span class='a'>x</span></div>" +
            "<div id='sibling'></div><span class='b'>x</span>" +
            "<div id='none'><section><span class='a'>x</span></section></div>");
        var child = FindBoxById(html, "child");
        var sibling = FindBoxById(html, "sibling");
        var none = FindBoxById(html, "none");
        Assert.AreNotEqual(RColor.Transparent, child.ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, sibling.ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, none.ActualBackgroundColor);
    }

    [TestMethod]
    [DataRow("div:has(.foo)")]
    public void Has_LeadingCombinator_RoundTripsThroughSelectorText(string selector)
    {
        var sheet = ParseStyleSheet($"{selector} {{ }}");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.AreEqual(selector, ((StyleRule)sheet.Rules[0]).SelectorText);
    }

    [TestMethod]
    [DataRow("div:has(> .foo)")]
    [DataRow("div:has(+ .foo)")]
    [DataRow("div:has(~ .foo)")]
    [Ignore("not yet spec compliant - see class doc comment: the leading combinator is discarded during " +
            "parsing, so ToCss() re-emits the selector without it and the round-trip text no longer matches.")]
    public void Has_LeadingCombinator_RoundTripsThroughSelectorText_LeadingCombinatorForms(string selector)
    {
        var sheet = ParseStyleSheet($"{selector} {{ }}");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.AreEqual(selector, ((StyleRule)sheet.Rules[0]).SelectorText);
    }

    [TestMethod]
    [Ignore("not yet spec compliant - see class doc comment: leading combinators on both comma-list " +
            "alternatives are discarded during parsing, so the round-trip text no longer matches.")]
    public void Has_MixedLeadingCombinatorsInCommaList_RoundTripsThroughSelectorText()
    {
        // ListSelector.ToCss doesn't insert a space after its comma (existing, pre-existing behavior -
        // see ListSelector.cs), so this asserts separately from the single-alternative cases above.
        var sheet = ParseStyleSheet("div:has(> .a, + .b) { }");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.AreEqual("div:has(> .a,+ .b)", ((StyleRule)sheet.Rules[0]).SelectorText);
    }

    // ── :where() ───────────────────────────────────────────────────────────────

    [TestMethod]
    public void Where_MatchesEitherBranch_LikeIs()
    {
        var html = Html(
            "p:where(.a, .b) { background-color: #ff0000; }",
            "<div><p class='a'>1</p><p class='b'>2</p><p>3</p></div>");
        var boxes = FindAllBoxesByTag(html, "p");
        Assert.AreEqual(3, boxes.Count);
        Assert.AreNotEqual(RColor.Transparent, boxes[0].ActualBackgroundColor);
        Assert.AreNotEqual(RColor.Transparent, boxes[1].ActualBackgroundColor);
        Assert.AreEqual(RColor.Transparent, boxes[2].ActualBackgroundColor);
    }

    [TestMethod]
    public void Where_ContributesZeroSpecificity_LosesToATypeSelector()
    {
        // ":where(#b)" matches, but per CSS Selectors 4 §16 it contributes ZERO specificity - so a
        // bare type selector "p" (0,0,1) outranks it even though ":where(#b)"'s argument is an id.
        // Contrast Is_SpecificityIsTheStaticMaxOfItsArguments (where ":is(#b)" WOULD win at one-id).
        var html = Html(
            ":where(#b) { color: #0000ff; } p { color: #ff0000; }",
            "<p id='b'>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(RColor.FromArgb(255, 0, 0), box.ActualColor);
    }

    [TestMethod]
    public void Where_StillMatches_WhenUncontested()
    {
        // Proves the zero-specificity rule above lost on specificity, not because :where() failed to
        // match: uncontested, the same ":where(#b)" rule applies its color.
        var html = Html(
            ":where(#b) { color: #0000ff; }",
            "<p id='b'>text</p>");
        var box = FindBoxByTag(html, "p");
        Assert.AreEqual(RColor.FromArgb(0, 0, 255), box.ActualColor);
    }

    // ── Helpers (mirrors CssSpecificityOrderingTests.cs conventions) ─────────

    private static string Html(string css, string body) =>
        $"<!DOCTYPE html><html><head><style>{css}</style></head><body>{body}</body></html>";

    private static CssBox FindBoxByTag(string html, string tag)
    {
        var (root, _) = LayoutHarness.Layout(html);
        var box = LayoutHarness.Descendants(root).FirstOrDefault(b => b.HtmlTag != null && b.HtmlTag.Name == tag);
        Assert.IsNotNull(box);
        return box!;
    }

    private static List<CssBox> FindAllBoxesByTag(string html, string tag)
    {
        var (root, _) = LayoutHarness.Layout(html);
        return LayoutHarness.Descendants(root).Where(b => b.HtmlTag != null && b.HtmlTag.Name == tag).ToList();
    }

    private static CssBox FindBoxById(string html, string id)
    {
        var (root, _) = LayoutHarness.Layout(html);
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box);
        return box!;
    }
}
